namespace wan24.CLI
{
    /// <summary>
    /// Interface for a CLI API helper
    /// </summary>
    public interface ICliApiHelper
    {
        /// <summary>
        /// Display context help
        /// </summary>
        /// <param name="context">Context</param>
        /// <returns>Exit code</returns>
        int DisplayHelp(CliApiContext context);
    }
}
