using System.Reflection;
using System.Text;

namespace wan24.CLI
{
    /// <summary>
    /// <see cref="string"/> extensions
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Remove the dash prefix from a CLI argument
        /// </summary>
        /// <param name="str">CLI argument</param>
        /// <returns>CLI argument without dash prefix</returns>
        public static string RemoveDashPrefix(this string str)
        {
            if (str.Length == 0 || str[0] != '-') return str;
            return str.Length > 1 && str[1] == '-' ? str[2..] : str[1..];
        }

        /// <summary>
        /// Append the command for a CLI API syntax information
        /// </summary>
        /// <param name="sb"><see cref="StringBuilder"/></param>
        /// <param name="app">App path or filename</param>
        internal static void AppendCommand(this StringBuilder sb, string? app = null)
        {
            if (app is null)
            {
                if (Assembly.GetEntryAssembly()?.Location is not string fn) return;
                app = Path.GetFileName(fn);
            }
            sb.Append(app.ToLower().EndsWith(".dll") ? $"[{CliApiInfo.RequiredColor}]dotnet {app}[/]" : $"[{CliApiInfo.RequiredColor}]{app}[/]");
            sb.Append(' ');
        }
    }
}
