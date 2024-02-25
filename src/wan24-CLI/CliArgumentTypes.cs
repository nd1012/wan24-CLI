using wan24.Core;

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
        [DisplayText("Boolean flag")]
        Flag,
        /// <summary>
        /// String value(s)
        /// </summary>
        [DisplayText("String value(s)")]
        Value,
        /// <summary>
        /// CLI argument object (see <see cref="ICliArguments"/>)
        /// </summary>
        [DisplayText("Argument object")]
        Object
    }
}
