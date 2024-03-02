using Microsoft.Extensions.Logging;
using Spectre.Console;
using wan24.Core;

namespace wan24.CLI
{
    /// <summary>
    /// Vivid console logger
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="level">Log level</param>
    /// <param name="next">Next logger</param>
    public class VividConsoleLogger(in LogLevel? level = null, in ILogger? next = null) : LoggerBase(level, next)
    {
        /// <summary>
        /// Write to STDERR?
        /// </summary>
        public bool WriteToStdErr { get; set; } = true;

        /// <summary>
        /// Trace log level color
        /// </summary>
        public string TraceColor { get; set; } = "grey on black";

        /// <summary>
        /// Debug log level color
        /// </summary>
        public string DebugColor { get; set; } = "silver on black";

        /// <summary>
        /// Information log level color
        /// </summary>
        public string InformationColor { get; set; } = "wheat1 on black";

        /// <summary>
        /// Warning log level color
        /// </summary>
        public string WarningColor { get; set; } = "yellow on black";

        /// <summary>
        /// Error log level color
        /// </summary>
        public string ErrorColor { get; set; } = "red on black";

        /// <summary>
        /// Critical log level color
        /// </summary>
        public string CriticalColor { get; set; } = "white on red";

        /// <inheritdoc/>
        protected override void LogInt<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            string color = logLevel switch
                {
                    LogLevel.Trace => TraceColor,
                    LogLevel.Debug => DebugColor,
                    LogLevel.Information => InformationColor,
                    LogLevel.Warning => WarningColor,
                    LogLevel.Error => ErrorColor,
                    LogLevel.Critical => CriticalColor,
                    _ => throw new InvalidProgramException()
                },
                message = $"[{color}]{GetMessage(logLevel, eventId, state, exception, formatter).EscapeMarkup()}[/]";
            if (WriteToStdErr)
            {
                CliApi.StdErr.MarkupLine(message);
            }
            else
            {
                AnsiConsole.MarkupLine(message);
            }
        }
    }
}
