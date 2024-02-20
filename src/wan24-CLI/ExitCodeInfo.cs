namespace wan24.CLI
{
    /// <summary>
    /// Exit code information
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="api">API</param>
    /// <param name="method">API method</param>
    /// <param name="attr">Attribute</param>
#pragma warning disable IDE1006 // Use variable names which start with upper case
    public record class ExitCodeInfo(CliApiInfo api, CliApiMethodInfo method, ExitCodeAttribute attr)
#pragma warning restore IDE1006 // Use variable names which start with upper case
    {
        /// <summary>
        /// API
        /// </summary>
        public CliApiInfo API { get; } = api;

        /// <summary>
        /// API method
        /// </summary>
        public CliApiMethodInfo Method { get; } = method;

        /// <summary>
        /// Attribute
        /// </summary>
        public ExitCodeAttribute Attribute { get; } = attr;

        /// <summary>
        /// Exit code
        /// </summary>
        public int Code { get; } = attr.Code;

        /// <summary>
        /// Description
        /// </summary>
        public string? Description { get; } = attr.Description;
    }
}
