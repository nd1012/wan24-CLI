namespace wan24.CLI
{
    /// <summary>
    /// Attribute for a CLI API method which uses STDERR output
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="description">Description</param>
    [AttributeUsage(AttributeTargets.Method)]
    public class StdErrAttribute(string description) : Attribute()
    {
        /// <summary>
        /// Description
        /// </summary>
        public string Description { get; } = description;
    }
}
