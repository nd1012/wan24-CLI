using System.Reflection;
using wan24.Core;
using static wan24.Core.TranslationHelper;

namespace wan24.CLI
{
    /// <summary>
    /// Attribute for a CLI API type, method and property and parameters
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="name">API (method/argument) name</param>
    /// <param name="parseJson">Parse JSON values?</param>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = true)]
    public class CliApiAttribute(string? name = null, bool parseJson = false) : Attribute()
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="keyLessOffset">Keyless argument offset (applies to a property or parameteter only)</param>
        /// <param name="parseJson">Parse JSON values? (applies to a property or parameteter only)</param>
        public CliApiAttribute(int keyLessOffset, bool parseJson = false) : this(parseJson: parseJson) => KeyLessOffset = keyLessOffset;

        /// <summary>
        /// API (method/argument) name
        /// </summary>
        public string Name { get; } = name ?? string.Empty;

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
        public bool ParseJson { get; set; } = parseJson;

        /// <summary>
        /// Example display value
        /// </summary>
        public string? Example { get; set; }

        /// <summary>
        /// Static help text property name (needs to return a <see cref="string"/>; Spectre.Console markup is supported)
        /// </summary>
        public string? HelpTextProperty { get; set; }

        /// <summary>
        /// Public static help method name (<see cref="CliHelpApi.DetailHelp_Delegate"/>)
        /// </summary>
        public string? HelpMethod { get; set; }

        /// <summary>
        /// Can an argument value be parsed using <see cref="ParseArgument(string, Type, string)"/>?
        /// </summary>
        public virtual bool CanParseArgument { get; }

        /// <summary>
        /// Get the help text (Spectre.Console markup is supported)
        /// </summary>
        /// <returns>Help text</returns>
        public virtual string? GetHelpText()
        {
            if (HelpTextProperty is null) return null;
            string[] temp = HelpTextProperty.Split('.');
            Type type = TypeHelper.Instance.GetType(string.Join('.', temp.SkipLast(count: 1)), throwOnError: true)
                ?? throw new InvalidProgramException($"Failed to load the help text from \"{HelpTextProperty}\": Failed to load the properties type");
            PropertyInfoExt pi = type.GetPropertyCached(temp[^1], BindingFlags.Public | BindingFlags.Static)
                ?? throw new InvalidProgramException($"Failed to load the help text from \"{HelpTextProperty}\": Property \"{temp[^1]}\" not found");
            if (pi.Property.PropertyType != typeof(string))
                throw new InvalidProgramException($"Failed to load the help text from \"{HelpTextProperty}\": Invalid property type {pi.Property.PropertyType} ({typeof(string)} expected)");
            if (pi.Getter is null)
                throw new InvalidProgramException($"Failed to load the help text from \"{HelpTextProperty}\": Property \"{temp[^1]}\" has no getter");
            return pi.Getter!(null) is string res ? _(res) : null;
        }

        /// <summary>
        /// Run the help method, if defined
        /// </summary>
        /// <param name="apiElement">API element</param>
        /// <param name="context">Context</param>
        /// <returns>If the method was executed</returns>
        public virtual bool RunHelpMethod(object apiElement, CliApiContext context)
        {
            if (HelpMethod is null) return false;
            string[] temp = HelpMethod.Split('.');
            Type type = TypeHelper.Instance.GetType(string.Join('.', temp.SkipLast(count: 1)), throwOnError: true)
                ?? throw new InvalidProgramException($"Failed to load the help text method from \"{HelpMethod}\": Failed to load the static methods type");
            MethodInfo mi = type.GetMethodCached(temp[^1], BindingFlags.Public | BindingFlags.Static)
                ?? throw new InvalidProgramException($"Failed to load the help text method from \"{HelpMethod}\": Method \"{temp[^1]}\" not found");
            mi.Invoke(obj: null, [apiElement, context]);
            return true;
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

        /// <summary>
        /// Parse an argument value
        /// </summary>
        /// <param name="name">Argument name (without dashes)</param>
        /// <param name="type">Argument type</param>
        /// <param name="arg">Argument string value</param>
        /// <returns>Parsed argument value of the given type</returns>
        /// <exception cref="NotSupportedException">The <see cref="CliApiAttribute"/> doesn't support argument parsing</exception>
        public virtual object? ParseArgument(string name, Type type, string arg) => throw new NotSupportedException();
    }
}
