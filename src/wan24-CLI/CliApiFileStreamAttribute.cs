using wan24.Core;

namespace wan24.CLI
{
    /// <summary>
    /// CLI API <see cref="FileStream"/> argument value attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    public class CliApiFileStreamAttribute : CliApiAttribute
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">CLI argument name</param>
        public CliApiFileStreamAttribute(in string? name = null) : base(name) { }

        /// <summary>
        /// File mode
        /// </summary>
        public FileMode Mode { get; set; } = FileMode.Open;

        /// <summary>
        /// File access
        /// </summary>
        public FileAccess Access { get; set; } = FileAccess.Read;

        /// <summary>
        /// File share
        /// </summary>
        public FileShare Share { get; set; } = FileShare.Read;

        /// <summary>
        /// Unix mode
        /// </summary>
        public UnixFileMode? UnixMode { get; set; }

        /// <summary>
        /// Overwrite? (clears file contents of an existing file)
        /// </summary>
        public bool Overwrite { get; set; }

        /// <inheritdoc/>
        public override bool CanParseArgument => true;

        /// <inheritdoc/>
        public override object? ParseArgument(string name, Type type, string arg) => FsHelper.CreateFileStream(arg, Mode, Access, Share, options: FileOptions.None, UnixMode, Overwrite);
    }
}
