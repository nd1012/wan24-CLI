using Spectre.Console;
using System.Diagnostics.Contracts;
using static wan24.Core.TranslationHelper;

namespace wan24.CLI
{
    /// <summary>
    /// CLI API default helper
    /// </summary>
    public class CliApiHelper : ICliApiHelper
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public CliApiHelper() { }

        /// <summary>
        /// Default instance
        /// </summary>
        public static CliApiHelper Default { get; } = new();

        /// <inheritdoc/>
        public virtual int DisplayHelp(CliApiContext context)
        {
            Contract.Assume(CliApi.ExportedApis is not null);
            CliApi.DisplayHelpHeader();
            CliArgException? argException = context.Exception as CliArgException;
            if (context.Exception is not null)
            {
                CliApi.StdErr.MarkupLine(argException is not null
                    ? $"[white on red]{_("Invalid argument")} \"{argException.ParamName.EscapeMarkup()}\": {context.Exception.Message.EscapeMarkup()}[/]"
                    : $"[white on red]{_("An exception has been catched")}: {context.Exception.ToString().EscapeMarkup()}[/]");
                CliApi.StdErr.WriteLine();
            }
            CliHelpApi help = new()
            {
                ApiName = context.API?.GetType().GetCliApiName(),
                ApiMethodName = context.Method?.GetCliApiMethodName()
            };
            help.Help();
            return 1;
        }
    }
}
