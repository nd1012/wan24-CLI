using wan24.Core;

namespace wan24.CLI
{
    /// <summary>
    /// CLI argument type enumeration
    /// </summary>
    public enum CliArgumentHosts
    {
        /// <summary>
        /// None (not found)
        /// </summary>
        [DisplayText("No argument host")]
        None,
        /// <summary>
        /// Type property
        /// </summary>
        [DisplayText("Object property")]
        Property,
        /// <summary>
        /// Method parameter
        /// </summary>
        [DisplayText("Method parameter")]
        Parameter
    }
}
