using wan24.Core;

namespace wan24.CLI
{
    /// <summary>
    /// <see cref="CliArguments"/> extensions
    /// </summary>
    public static class CliArgumentsExtensions
    {
        /// <summary>
        /// Get an argument type
        /// </summary>
        /// <param name="ca">CLI arguments</param>
        /// <param name="arg">Argument name (excluding dash prefix)</param>
        /// <returns>Argument type or <see langword="null"/>, if the argument wasn't found</returns>
        public static CliArgumentTypes? GetArgumentType(this CliArguments ca, string arg)
        {
            if (!ca[arg]) return null;
            return ca.IsBoolean(arg) ? CliArgumentTypes.Flag : CliArgumentTypes.Value;
        }
    }
}
