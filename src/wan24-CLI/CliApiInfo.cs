using Spectre.Console;
using System.Collections.Frozen;
using System.Reflection;
using wan24.Core;

namespace wan24.CLI
{
    /// <summary>
    /// CLI API informations
    /// </summary>
    public record class CliApiInfo
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="api">CLI API type</param>
        /// <param name="nic"><see cref="NullabilityInfoContext"/></param>
        public CliApiInfo(Type api, NullabilityInfoContext? nic = null)
        {
            Type = api;
            Attribute = api.GetCustomAttributeCached<CliApiAttribute>();
            Name = api.GetCliApiName();
            Title = api.GetCliTitle();
            Description = api.GetCliDescription();
            IsErrorHandler = typeof(ICliApiErrorHandler).IsAssignableFrom(api);
            IsHelpProvider = typeof(ICliApiHelpProvider).IsAssignableFrom(api);
            nic ??= new();
            Dictionary<string, CliApiMethodInfo> methods = [];
            foreach (MethodInfo mi in api.GetExportedApiMethods())
                methods[mi.GetCliApiMethodName()] = mi.GetCustomAttributeCached<CliApiAttribute>()?.GetApiMethodInfo(this, mi, nic) ?? new(this, mi, nic);
            Methods = methods.ToFrozenDictionary();
        }

        /// <summary>
        /// Background color
        /// </summary>
        public static Color BackGroundColor { get; set; } = Color.Black;

        /// <summary>
        /// Required color
        /// </summary>
        public static Color RequiredColor { get; set; } = Color.Wheat1;

        /// <summary>
        /// Optional color
        /// </summary>
        public static Color OptionalColor { get; set; } = Color.Silver;

        /// <summary>
        /// Decoration color
        /// </summary>
        public static Color DecorationColor { get; set; } = Color.Grey;

        /// <summary>
        /// API name color
        /// </summary>
        public static Color ApiNameColor { get; set; } = Color.Aqua;

        /// <summary>
        /// API method name color
        /// </summary>
        public static Color ApiMethodNameColor { get; set; } = Color.Yellow;

        /// <summary>
        /// CLI API type
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// <see cref="CliApiAttribute"/>
        /// </summary>
        public CliApiAttribute? Attribute { get; }

        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// API methods (key is the method name)
        /// </summary>
        public FrozenDictionary<string, CliApiMethodInfo> Methods { get; }

        /// <summary>
        /// Title
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// Description
        /// </summary>
        public string? Description { get; }

        /// <summary>
        /// Is an error handler?
        /// </summary>
        public bool IsErrorHandler { get; }

        /// <summary>
        /// Is a help provider?
        /// </summary>
        public bool IsHelpProvider { get; }

        /// <summary>
        /// Default API method
        /// </summary>
        public CliApiMethodInfo? DefaultMethod => Methods.Values.FirstOrDefault(m => m.Attribute?.IsDefault ?? false) ?? Methods.Values.FirstOrDefault();

        /// <summary>
        /// Create for exported APIs
        /// </summary>
        /// <param name="exportedApis">Exported APIs</param>
        /// <returns>Exported API informations</returns>
        public static FrozenDictionary<string, CliApiInfo> Create(params Type[] exportedApis)
        {
            NullabilityInfoContext nic = new();
            Dictionary<string, CliApiInfo> res = new(exportedApis.Length);
            foreach (Type api in exportedApis) res[api.GetCliApiName()] = api.GetCustomAttributeCached<CliApiAttribute>()?.GetApiInfo(api, nic) ?? new(api, nic);
            return res.ToFrozenDictionary();
        }
    }
}
