using System.Reflection;
using wan24.Core;

namespace wan24.CLI
{
    /// <summary>
    /// Attribute for a CLI API type, method and property and parameters
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = true)]
    public class CliApiAttribute : Attribute
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">API (method/argument) name</param>
        /// <param name="parseJson">Parse JSON values?</param>
        public CliApiAttribute(string? name = null, bool parseJson = false) : base()
        {
            Name = name ?? string.Empty;
            ParseJson = parseJson;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="keyLessOffset">Keyless argument offset (applies to a property or parameteter only)</param>
        /// <param name="parseJson">Parse JSON values? (applies to a property or parameteter only)</param>
        public CliApiAttribute(int keyLessOffset, bool parseJson = false) : this(parseJson: parseJson) => KeyLessOffset = keyLessOffset;

        /// <summary>
        /// API (method/argument) name
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Is the default API (method)?
        /// </summary>
        public bool IsDefault { get; set; }

        /// <summary>
        /// Keyless argument offset (applies to a property or parameteter only)
        /// </summary>
        public int KeyLessOffset { get; } = -1;

        /// <summary>
        /// Parse JSON values? (applies to a property or parameteter only)
        /// </summary>
        public bool ParseJson { get; set; }

        /// <summary>
        /// Static help text property name (needs to return a <see cref="string"/>)
        /// </summary>
        public string? HelpTextProperty { get; set; }

        /// <summary>
        /// If the help text is MarkDown formatted
        /// </summary>
        public bool HelpTextIsMarkDown { get; set; }

        /// <summary>
        /// Get the help text
        /// </summary>
        /// <returns>Help text</returns>
        public virtual string? GetHelpText()
        {
            if (HelpTextProperty is null) return null;
            string[] temp = HelpTextProperty.Split('.');
            Type type = TypeHelper.Instance.GetType(string.Join('.', temp.AsSpan(0, temp.Length - 1).ToArray()), throwOnError: true)
                ?? throw new InvalidProgramException($"Failed to load the help text from \"{HelpTextProperty}\": Failed to load the properties type");
            PropertyInfoExt pi = type.GetPropertyCached(temp[^1], BindingFlags.Public | BindingFlags.Static)
                ?? throw new InvalidProgramException($"Failed to load the help text from \"{HelpTextProperty}\": Property \"{temp[^1]}\" not found");
            if (pi.Property.PropertyType != typeof(string))
                throw new InvalidProgramException($"Failed to load the help text from \"{HelpTextProperty}\": Invalid property type {pi.Property.PropertyType} ({typeof(string)} expected)");
            if (!pi.Property.CanRead)
                throw new InvalidProgramException($"Failed to load the help text from \"{HelpTextProperty}\": Property \"{temp[^1]}\" has no getter");
            return pi.Getter!(null) as string;
        }

        /// <summary>
        /// Get API informations
        /// </summary>
        /// <param name="api">API type</param>
        /// <param name="nic"><see cref="NullabilityInfoContext"/></param>
        /// <returns>API informations</returns>
        public virtual CliApiInfo? GetApiInfo(Type api, NullabilityInfoContext nic) => null;

        /// <summary>
        /// Get API method informations
        /// </summary>
        /// <param name="api">API</param>
        /// <param name="mi">Method</param>
        /// <param name="nic"><see cref="NullabilityInfoContext"/></param>
        /// <returns>API method informations</returns>
        public virtual CliApiMethodInfo? GetApiMethodInfo(CliApiInfo api, MethodInfo mi, NullabilityInfoContext nic) => null;

        /// <summary>
        /// Get API argument informations
        /// </summary>
        /// <param name="method">Method</param>
        /// <param name="pi">Property</param>
        /// <param name="nic"><see cref="NullabilityInfoContext"/></param>
        /// <returns>API argument informations</returns>
        public virtual CliApiArgumentInfo? GetArgumentInfo(CliApiMethodInfo method, PropertyInfo pi, NullabilityInfoContext nic) => null;

        /// <summary>
        /// Get API argument informations
        /// </summary>
        /// <param name="method">Method</param>
        /// <param name="pi">Parameter</param>
        /// <param name="nic"><see cref="NullabilityInfoContext"/></param>
        /// <returns>API argument informations</returns>
        public virtual CliApiArgumentInfo? GetArgumentInfo(CliApiMethodInfo method, ParameterInfo pi, NullabilityInfoContext nic) => null;
    }
}
