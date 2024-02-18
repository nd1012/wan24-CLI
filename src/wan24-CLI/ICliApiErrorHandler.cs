namespace wan24.CLI
{
    /// <summary>
    /// Interface for a CLI API which implements error handling
    /// </summary>
    public interface ICliApiErrorHandler
    {
        /// <summary>
        /// Handle an API error
        /// </summary>
        /// <param name="context">CLI API context</param>
        /// <returns>Exit code</returns>
        Task<int> HandleApiErrorAsync(CliApiContext context);
    }
}
