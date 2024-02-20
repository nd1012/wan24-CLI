namespace wan24.CLI
{
    /// <summary>
    /// Attribute for a CLI API method which uses STDIN input
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="description">Description</param>
    /// <param name="required">Is required?</param>
    [AttributeUsage(AttributeTargets.Method)]
    public class StdInAttribute(string description, bool required = false) : Attribute()
    {
        /// <summary>
        /// Description
        /// </summary>
        public string Description { get; } = description;

        /// <summary>
        /// Is required?
        /// </summary>
        public bool Required { get; } = required;
    }
}
