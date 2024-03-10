using Spectre.Console;
using System.Diagnostics.Contracts;
using wan24.Core;
using static wan24.Core.TranslationHelper.Ext;

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
                    : $"[white on red]{_("An exception has been catched")}: {(CliApi.DisplayFullExceptions ? context.Exception.ToString().EscapeMarkup() : context.Exception.Message.EscapeMarkup())}[/]");
                CliApi.StdErr.WriteLine();
            }
            CliHelpApi help = CliApi.ExportedApis.Values.Where(a => typeof(CliHelpApi).IsAssignableFrom(a.Type)).FirstOrDefault()?.Type is Type apiHelpType
                ? apiHelpType.ConstructAuto() as CliHelpApi ?? throw new InvalidProgramException($"Failed to instance API help from {apiHelpType}")
                : new();
            try
            {
                help.ApiName = context.API?.GetType().GetCliApiName();
                help.ApiMethodName = context.Method?.GetCliApiMethodName();
                help.Help();
            }
            finally
            {
                help.TryDispose();
            }
            return 1;
        }
    }
}
