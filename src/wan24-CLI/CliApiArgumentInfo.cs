using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using wan24.Core;

namespace wan24.CLI
{
    /// <summary>
    /// CLI API argument informations
    /// </summary>
    public partial record class CliApiArgumentInfo
    {
        /// <summary>
        /// Regular expression to match double spaces
        /// </summary>
        private static readonly Regex RX_DOUBLE_SPACE = RxDoubleSpace();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="method">Method</param>
        /// <param name="pi">Property</param>
        /// <param name="nic"><see cref="NullabilityInfoContext"/></param>
        public CliApiArgumentInfo(CliApiMethodInfo method, PropertyInfoExt pi, NullabilityInfoContext nic)
        {
            Method = method;
            Host = CliArgumentHosts.Property;
            Property = pi;
            Name = pi.Property.GetCliApiArgumentName().RemoveDashPrefix();
            Attribute = pi.GetCustomAttributeCached<CliApiAttribute>() ?? throw new ArgumentException("Missing CliApiAttribute", nameof(pi));
            Type = ClrType.GetCliArgumentType();
            IsValueList = pi.PropertyType.IsArray;
            IsRequired = pi.Property.IsCliValueRequired(nic);
            IsComplex = Type == CliArgumentTypes.Value && ((IsValueList && pi.PropertyType.GetElementType() != typeof(string)) || (!IsValueList && pi.PropertyType != typeof(string)));
            Title = pi.Property.GetCliTitle();
            Description = pi.Property.GetCliDescription();
            SetObjectProperties(nic);
        }

        /// <summary>
        /// COnstructor
        /// </summary>
        /// <param name="method">Method</param>
        /// <param name="pi">Parameter</param>
        /// <param name="nic"><see cref="NullabilityInfoContext"/></param>
        public CliApiArgumentInfo(CliApiMethodInfo method, ParameterInfo pi, NullabilityInfoContext nic)
        {
            Method = method;
            Host = CliArgumentHosts.Property;
            Parameter = pi;
            Name = pi.GetCliApiArgumentName().RemoveDashPrefix();
            Attribute = pi.GetCustomAttributeCached<CliApiAttribute>() ?? throw new ArgumentException("Missing CliApiAttribute", nameof(pi));
            Type = ClrType.GetCliArgumentType();
            IsValueList = pi.ParameterType.IsArray;
            IsRequired = pi.IsCliValueRequired(nic);
            IsComplex = Type == CliArgumentTypes.Value && ((IsValueList && pi.ParameterType.GetElementType() != typeof(string)) || (!IsValueList && pi.ParameterType != typeof(string)));
            Title = pi.GetCliTitle();
            Description = pi.GetCliDescription();
            SetObjectProperties(nic);
        }

        /// <summary>
        /// Method
        /// </summary>
        public CliApiMethodInfo Method { get; }

        /// <summary>
        /// Host type
        /// </summary>
        public CliArgumentHosts Host { get; }

        /// <summary>
        /// Property
        /// </summary>
        public PropertyInfoExt? Property { get; }

        /// <summary>
        /// Parameter
        /// </summary>
        public ParameterInfo? Parameter { get; }

        /// <summary>
        /// <see cref="CliApiAttribute"/>
        /// </summary>
        public CliApiAttribute Attribute { get; }

        /// <summary>
        /// Argument object properties
        /// </summary>
        public IReadOnlyDictionary<string, CliApiArgumentInfo>? ObjectProperties { get; protected set; }

        /// <summary>
        /// Argument type
        /// </summary>
        public CliArgumentTypes Type { get; }

        /// <summary>
        /// Value CLR type
        /// </summary>
        public Type ClrType => Property?.PropertyType ?? Parameter?.ParameterType!;

        /// <summary>
        /// Is a value list?
        /// </summary>
        public bool IsValueList { get; }

        /// <summary>
        /// Is a keyless argument?
        /// </summary>
        public bool IsKeyLess => Attribute.KeyLessOffset != -1;

        /// <summary>
        /// Is required?
        /// </summary>
        public bool IsRequired { get; }

        /// <summary>
        /// Is a complex value type which requires JSON parsing?
        /// </summary>
        public bool IsComplex { get; }

        /// <summary>
        /// Name (excluding dash prefix)
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Argument name (including dash prefix)
        /// </summary>
        public string ArgumentName
        {
            get
            {
                this.EnsureValidState(Type != CliArgumentTypes.Object, "Not an argument");
                return Type switch
                {
                    CliArgumentTypes.Flag => $"-{Name}",
                    CliArgumentTypes.Value => $"--{Name}",
                    _ => throw new InvalidProgramException()
                };
            }
        }

        /// <summary>
        /// Get the CLR name (used for a keyless argument)
        /// </summary>
        public string ClrName => Property?.Name ?? Parameter!.Name!;

        /// <summary>
        /// Title
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// Description
        /// </summary>
        public string? Description { get; }

        /// <inheritdoc/>
        public override string ToString()
        {
            if (Type == CliArgumentTypes.Object)
            {
                StringBuilder sb = new();
                foreach (CliApiArgumentInfo ai in ObjectProperties!.Values)
                {
                    sb.Append(ai.ToString());
                    sb.Append(' ');
                }
                if (sb.Length > 0) sb.Remove(sb.Length - 1, 1);
                return RX_DOUBLE_SPACE.Replace(sb.ToString(), " ");
            }
            if (IsKeyLess)
                return IsRequired
                    ? $"[{CliApiInfo.RequiredColor}]{Name}({ClrName}){(ClrType.IsArray ? " ..." : string.Empty)}[/]"
                    : $"[{CliApiInfo.DecorationColor}]([/][{CliApiInfo.OptionalColor}]{Name}({ClrName}){(ClrType.IsArray ? " ..." : string.Empty)}[/][{CliApiInfo.DecorationColor}])[/]";
            return Type switch
            {
                CliArgumentTypes.Flag => $"[{CliApiInfo.DecorationColor}]([/][{CliApiInfo.OptionalColor}]{ArgumentName}[/][{CliApiInfo.DecorationColor}])[/]",
                CliArgumentTypes.Value => IsRequired
                    ? $"[{CliApiInfo.RequiredColor}]{ArgumentName}[/] [{CliApiInfo.DecorationColor}]{(ClrType.IsArray ? $"{ClrName} ..." : ClrName)}[/]"
                    : $"[{CliApiInfo.DecorationColor}]([/][{CliApiInfo.OptionalColor}]{ArgumentName}[/] [{CliApiInfo.DecorationColor}]{(ClrType.IsArray ? $"{ClrName} ..." : ClrName)}[/][{CliApiInfo.DecorationColor}])[/]",
                _ => throw new InvalidProgramException()
            };
        }

        /// <summary>
        /// Set the <see cref="ObjectProperties"/> value
        /// </summary>
        /// <param name="nic"><see cref="NullabilityInfoContext"/></param>
        protected virtual void SetObjectProperties(NullabilityInfoContext nic)
        {
            if (Type != CliArgumentTypes.Object)
            {
                ObjectProperties = null;
                return;
            }
            Dictionary<string, CliApiArgumentInfo> props = [];
            CliApiArgumentInfo info;
            foreach (PropertyInfoExt pi in CliApi.FindApiArguments(ClrType))
            {
                info = new(Method, pi, nic);
                props[info.Name] = info;
            }
            ObjectProperties = props;
        }

        /// <summary>
        /// Matches double spaces
        /// </summary>
        /// <returns>Regular expression</returns>
        [GeneratedRegex(@"\s{2,}", RegexOptions.Compiled)]
        private static partial Regex RxDoubleSpace();
    }
}
