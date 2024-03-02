namespace wan24.CLI
{
    /// <summary>
    /// Thrown on CLI argument error
    /// </summary>
    [Serializable]
    public class CliArgException : ArgumentException
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public CliArgException() : base() { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message">Message</param>
        public CliArgException(string? message) : base(message) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="inner">Inner exception</param>
        public CliArgException(string? message, Exception? inner) : base(message, inner) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="argName">Argument name</param>
        public CliArgException(string? message, string? argName) : base(message, argName) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="argName">Argument name</param>
        /// <param name="inner">Inner exception</param>
        public CliArgException(string? message, string? argName, Exception? inner) : base(message, argName, inner) { }
    }
}
