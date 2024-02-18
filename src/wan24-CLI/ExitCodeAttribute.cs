namespace wan24.CLI
{
    /// <summary>
    /// Attrbute for documenting CLI API method exit codes
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="code">Exit code</param>
    /// <param name="description">Description</param>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class ExitCodeAttribute(int code, string? description = null) : Attribute()
    {
        /// <summary>
        /// Exit code
        /// </summary>
        public int Code { get; } = code;

        /// <summary>
        /// Description
        /// </summary>
        public string? Description { get; } = description;
    }
}
