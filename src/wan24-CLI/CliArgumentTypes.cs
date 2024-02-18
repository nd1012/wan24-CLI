namespace wan24.CLI
{
    /// <summary>
    /// CLI argument types
    /// </summary>
    public enum CliArgumentTypes
    {
        /// <summary>
        /// Boolean (flag)
        /// </summary>
        Flag,
        /// <summary>
        /// String value(s)
        /// </summary>
        Value,
        /// <summary>
        /// CLI argument object (see <see cref="ICliArguments"/>)
        /// </summary>
        Object
    }
}
