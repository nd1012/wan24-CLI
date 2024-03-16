using System.ComponentModel;
using System.Reflection;
using wan24.Core;

namespace wan24.CLI
{
    /// <summary>
    /// App version CLI API
    /// </summary>
    [CliApi("version")]
    [DisplayText("Version")]
    [Description("Display the app version")]
    public sealed class VersionApi
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public VersionApi() { }

        /// <summary>
        /// App version
        /// </summary>
        public static Version Version { get; set; }
            = new Version(Assembly.GetEntryAssembly()?.GetCustomAttributeCached<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "1.0.0");

        /// <summary>
        /// App title
        /// </summary>
        public static string Title { get; set; } = Settings.AppId;

        /// <summary>
        /// Display the app version
        /// </summary>
        [CliApi("display", IsDefault = true)]
        [DisplayText("Display")]
        [Description("Display the app version")]
        public static void DisplayVersion() => Console.WriteLine($"{Title} version {Version}");
    }
}
