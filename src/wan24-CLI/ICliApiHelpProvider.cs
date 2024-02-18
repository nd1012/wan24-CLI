namespace wan24.CLI
{
    /// <summary>
    /// Interface for a CLI API which provides context help
    /// </summary>
    public interface ICliApiHelpProvider
    {
        /// <summary>
        /// Display context help
        /// </summary>
        /// <param name="context">CLI API context</param>
        /// <returns>Exit code</returns>
        int DisplayHelp(CliApiContext context);
    }
}
