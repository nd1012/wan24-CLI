using wan24.Core;

[assembly: Bootstrapper(typeof(wan24.CLI.Bootstrapper), nameof(wan24.CLI.Bootstrapper.Boot))]

namespace wan24.CLI
{
    /// <summary>
    /// Bootstrapper
    /// </summary>
    public static class Bootstrapper
    {
        /// <summary>
        /// Boot
        /// </summary>
        public static void Boot()
        {
            StatusProviderTable.Providers["CLI API"] = CliApi.State;
        }
    }
}
