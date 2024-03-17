using Spectre.Console;
using System.ComponentModel;
using System.Reflection;
using wan24.Core;

namespace wan24.CLI
{
    /// <summary>
    /// About CLI API
    /// </summary>
    [CliApi("about")]
    [DisplayText("About")]
    [Description("Display informations about this CLI app")]
    public sealed class AboutApi
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public AboutApi() { }

        /// <summary>
        /// App title
        /// </summary>
        public static string Title { get; set; } = Settings.AppId;

        /// <summary>
        /// App informations to display (Spectre.Console markup is supported)
        /// </summary>
        public static string? Info { get; set; }

        /// <summary>
        /// App version
        /// </summary>
        public static Version Version { get; set; }
            = new Version(Assembly.GetEntryAssembly()?.GetCustomAttributeCached<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "1.0.0");

        /// <summary>
        /// Display the app informations
        /// </summary>
        [CliApi("info", IsDefault = true)]
        [DisplayText("Informations")]
        [Description("Display detailed app informations")]
        public static void DisplayInfo()
        {
            DisplayVersion();
            if (Info is null) return;
            Console.WriteLine();
            AnsiConsole.WriteLine(Info);
        }

        /// <summary>
        /// Display the app version
        /// </summary>
        [CliApi("version")]
        [DisplayText("Version")]
        [Description("Display app version informations")]
        public static void DisplayVersion() => Console.WriteLine($"{Title} version {Version} ({(ENV.IsDebug ? "Debug build" : "Release build")})");
    }
}
