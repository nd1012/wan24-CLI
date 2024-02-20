using System.Text;
using System.Text.RegularExpressions;

namespace wan24.CLI
{
    /// <summary>
    /// CLI API informations extensions
    /// </summary>
    public static partial class CliApiInfosExtensions
    {
        /// <summary>
        /// Regular expression to match double spaces
        /// </summary>
        private static readonly Regex RX_DOUBLE_SPACES = RxDoubleSpaces();

        /// <summary>
        /// Get the default API
        /// </summary>
        /// <param name="infos">API infos</param>
        /// <returns>Default API</returns>
        public static CliApiInfo? GetDefaultApi(this IReadOnlyDictionary<string, CliApiInfo> infos)
            => infos.Values.FirstOrDefault(a => a.Attribute?.IsDefault ?? false) ?? infos.Values.FirstOrDefault();

        /// <summary>
        /// Determine if the CLI help API is served
        /// </summary>
        /// <param name="exportedApis">Exported API types</param>
        /// <returns>If the CLI help API is being served</returns>
        public static bool IsCliHelpApiServed(this IEnumerable<Type> exportedApis) => exportedApis.Any(t => typeof(CliHelpApi).IsAssignableFrom(t));

        /// <summary>
        /// Determine if the CLI help API is served
        /// </summary>
        /// <param name="apis">API infos</param>
        /// <returns>If the CLI help API is being served</returns>
        public static bool IsCliHelpApiServed(this IEnumerable<CliApiInfo> apis) => apis.Any(a => typeof(CliHelpApi).IsAssignableFrom(a.Type));

        /// <summary>
        /// Determine if the CLI help API is served
        /// </summary>
        /// <param name="apis">API infos</param>
        /// <returns>If the CLI help API is being served</returns>
        public static bool IsCliHelpApiServed(this IReadOnlyDictionary<string, CliApiInfo> apis) => apis.Values.IsCliHelpApiServed();

        /// <summary>
        /// Get the API method call syntax
        /// </summary>
        /// <param name="infos">API informations</param>
        /// <param name="api">API name</param>
        /// <param name="method">Method name</param>
        /// <param name="app">CLI app command to use</param>
        /// <returns>Syntax (with <see cref="Spectre.Console.Markup"/>)</returns>
        public static string GetApiMethodSyntax(this IReadOnlyDictionary<string, CliApiInfo> infos, string? api = null, string? method = null, string? app = null)
        {
            api ??= infos.GetDefaultApi()?.Name ?? throw new ArgumentNullException(nameof(api));
            if (!infos.TryGetValue(api, out CliApiInfo? apiInfo)) throw new ArgumentException("Unknown API", nameof(api));
            method ??= apiInfo.DefaultMethod?.Name ?? throw new ArgumentNullException(nameof(method));
            if (!apiInfo.Methods.TryGetValue(method, out CliApiMethodInfo? methodInfo)) throw new ArgumentException("Unknown method", nameof(method));
            StringBuilder sb = new($"[on {CliApiInfo.BackGroundColor}]");
            sb.AppendCommand(app);
            if (infos.Count != 1)
            {
                sb.Append($"[{CliApiInfo.ApiNameColor}]{api}[/]");
                sb.Append(' ');
            }
            if (apiInfo.Methods.Count != 1)
            {
                sb.Append($"[{CliApiInfo.ApiMethodNameColor}]{method}[/]");
                sb.Append(' ');
            }
            sb.Append(methodInfo.ToString());
            sb.Append("[/]");
            return RX_DOUBLE_SPACES.Replace(sb.ToString(), " ");
        }

        /// <summary>
        /// Regular expression to match double spaces
        /// </summary>
        /// <returns>Regular expression</returns>
        [GeneratedRegex(@"\s{2,}", RegexOptions.Compiled)]
        private static partial Regex RxDoubleSpaces();
    }
}
