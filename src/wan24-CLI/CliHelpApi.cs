using Spectre.Console;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using wan24.Core;
using wan24.ObjectValidation;
using static wan24.Core.TranslationHelper;

namespace wan24.CLI
{
    /// <summary>
    /// CLI help API
    /// </summary>
    [CliApi("help")]
    [DisplayText("CLI help API")]
    [Description("Provides generated general usage instructions for a CLI API")]
    public class CliHelpApi : ICliApiHelpProvider, ICliApiHelper, ICliApiErrorHandler
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public CliHelpApi() { }

        /// <summary>
        /// API method name
        /// </summary>
        [CliApi("api"), RequiredIf(nameof(ApiMethodName))]
        [DisplayText("Name of the API to display help for")]
        [Description("Add the API name to the command to specify the API to use and display available method informations")]
        public string? ApiName { get; set; }

        /// <summary>
        /// API method name
        /// </summary>
        [CliApi("method")]
        [DisplayText("Name of the API method to display help for")]
        [Description("Add the API method name to the command to specify the API method to use and display available argument informations")]
        public string? ApiMethodName { get; set; }

        /// <summary>
        /// Print details, if available?
        /// </summary>
        [CliApi]
        [DisplayText("Show help details, if available")]
        [Description("Some APIs/methods/arguments may provide detailed usage informations on request")]
        public bool Details { get; set; }

        /// <summary>
        /// Help
        /// </summary>
        /// <returns>Exit code <c>0</c></returns>
        [CliApi(IsDefault = true)]
        [DisplayText("Display CLI API help")]
        [Description("Displays the usage instructions based on the given arguments (without any following arguments available APIs are being listed)")]
        [ExitCode(0, "Default exit code")]
        public virtual int Help()
        {
            Contract.Assert(CliApi.CurrentContext is not null && CliApi.ExportedApis is not null);
            CliApi.DisplayHelpHeader();
            CliApiInfo? apiInfo = ApiName is null
                ? null
                : CliApi.ExportedApis.Values.FirstOrDefault(a => a.Name.Equals(ApiName, StringComparison.OrdinalIgnoreCase));
            CliApiMethodInfo? methodInfo = apiInfo is null || ApiMethodName is null
                ? null
                : apiInfo.Methods.Values.FirstOrDefault(m => m.Name.Equals(ApiMethodName, StringComparison.OrdinalIgnoreCase));
            if (ApiName is null || apiInfo is null)
            {
                if (ApiName is not null)
                {
                    CliApi.StdErr.MarkupLine($"[white on red]{_("API not found")}: \"{ApiName}\"[/]");
                    CliApi.StdErr.WriteLine();
                }
                AnsiConsole.MarkupLine($"[{CliApiInfo.HighlightColor}]{_("Usage")}:[/]");
                AnsiConsole.MarkupLine($"  [{CliApiInfo.RequiredColor}]{CliApi.CommandLine.EscapeMarkup()}[/] [{CliApiInfo.ApiNameColor}]API[/] [{CliApiInfo.DecorationColor}]([/][{CliApiInfo.ApiMethodNameColor}]{_("Method")}[/][{CliApiInfo.DecorationColor}]) ([/][{CliApiInfo.OptionalColor}]{_("Arguments")}[/][{CliApiInfo.DecorationColor}])[/]");
                Console.WriteLine();
                AnsiConsole.MarkupLine($"[{CliApiInfo.HighlightColor}]{_("Available APIs")}:[/]");
                foreach (CliApiInfo cliApiInfo in CliApi.ExportedApis.Values)
                {
                    Console.WriteLine();
                    AnsiConsole.Markup($"  [{CliApiInfo.ApiNameColor}]{cliApiInfo.Name}[/]: [{CliApiInfo.DecorationColor}]{_(cliApiInfo.Title.EscapeMarkup())}[/]");
                    if ((cliApiInfo.Attribute?.IsDefault ?? false) || cliApiInfo.Type == CliApi.CurrentContext.ExportedApis.FirstOrDefault(a => !typeof(CliHelpApi).IsAssignableFrom(a)))
                        AnsiConsole.Markup($"[{CliApiInfo.OptionalColor}] ({_("Default")})[/]");
                    Console.WriteLine();
                    if (cliApiInfo.Description is not null) AnsiConsole.MarkupLine($"  {_(cliApiInfo.Description.EscapeMarkup())}");
                    if (!Details) continue;
                    Console.WriteLine();
                    AnsiConsole.MarkupLine($"  {_("API methods")}:");
                    foreach (CliApiMethodInfo apiMethodInfo in cliApiInfo.Methods.Values)
                    {
                        Console.WriteLine();
                        AnsiConsole.MarkupLine($"    [{CliApiInfo.ApiMethodNameColor}]{apiMethodInfo.Name}[/] {apiMethodInfo}");
                        Console.Write("    ");
                        if (apiMethodInfo == cliApiInfo.DefaultMethod || apiMethodInfo == cliApiInfo.Methods.Values.First())
                            AnsiConsole.Markup($"[{CliApiInfo.OptionalColor}]({_("Default")}) [/]");
                        AnsiConsole.MarkupLine($"[{CliApiInfo.RequiredColor}]{_(apiMethodInfo.Title.EscapeMarkup())}[/]");
                        if (apiMethodInfo.Description is not null) AnsiConsole.MarkupLine($"    {_(apiMethodInfo.Description.EscapeMarkup())}");
                    }
                }
                if (CliApi.ExportedApis.IsCliHelpApiServed())
                {
                    Console.WriteLine();
                    Console.WriteLine($"{_("To display detailed help")}:");
                    Console.WriteLine();
                    AnsiConsole.MarkupLine($"  [{CliApiInfo.RequiredColor}]{CliApi.CommandLine.EscapeMarkup()} help[/] [{CliApiInfo.DecorationColor}]([/]--api [{CliApiInfo.ApiNameColor}]API[/][{CliApiInfo.DecorationColor}])[/] [{CliApiInfo.DecorationColor}]([/]--method [{CliApiInfo.ApiMethodNameColor}]{_("Method")}[/][{CliApiInfo.DecorationColor}]) ([/][{CliApiInfo.OptionalColor}]-Details[/][{CliApiInfo.DecorationColor}])[/]");
                    if (!Details)
                    {
                        Console.WriteLine();
                        AnsiConsole.MarkupLine(_("The optional [%{0}]-Details[/] flag forces the help API to output more details.", CliApiInfo.OptionalColor.ToString()));
                    }
                }
            }
            else if (ApiMethodName is null || methodInfo is null)
            {
                if (ApiMethodName is not null)
                {
                    CliApi.StdErr.MarkupLine($"[white on red]{_("API method not found")}: \"{ApiName}::{ApiMethodName}\"[/]");
                    CliApi.StdErr.WriteLine();
                }
                AnsiConsole.Markup($"{_("Help for API")} [{CliApiInfo.ApiNameColor}]{ApiName}[/]: [{CliApiInfo.DecorationColor}]{_(apiInfo.Title.EscapeMarkup())}[/]");
                if ((apiInfo.Attribute?.IsDefault ?? false) || CliApi.CurrentContext.API!.GetType() == CliApi.CurrentContext.ExportedApis.FirstOrDefault(a => !typeof(CliHelpApi).IsAssignableFrom(a)))
                    AnsiConsole.Markup($"[{CliApiInfo.OptionalColor}] ({_("Default")})[/]");
                Console.WriteLine();
                if (apiInfo.Description is not null)
                {
                    Console.WriteLine();
                    AnsiConsole.MarkupLine(_(apiInfo.Description.EscapeMarkup()));
                }
                Console.WriteLine();
                AnsiConsole.MarkupLine($"[{CliApiInfo.HighlightColor}]{_("Usage")}:[/]");
                AnsiConsole.MarkupLine($"  [{CliApiInfo.RequiredColor}]{CliApi.CommandLine.EscapeMarkup()}[/] [{CliApiInfo.ApiNameColor}]{ApiName}[/] [{CliApiInfo.DecorationColor}]([/][{CliApiInfo.ApiMethodNameColor}]{_("Method")}[/][{CliApiInfo.DecorationColor}]) ([/][{CliApiInfo.OptionalColor}]{_("Arguments")}[/][{CliApiInfo.DecorationColor}])[/]");
                Console.WriteLine();
                if (Details && (apiInfo.Attribute?.HelpTextProperty is not null || apiInfo.Attribute?.HelpMethod is not null))
                {
                    AnsiConsole.MarkupLine($"[{CliApiInfo.HighlightColor}]{_("Usage details")}:[/]");
                    if (apiInfo.Attribute?.HelpTextProperty is not null)
                    {
                        AnsiConsole.MarkupLine(apiInfo.Attribute.GetHelpText()!.Parse(new Dictionary<string, string>()
                        {
                            {"CommandLine", CliApi.CommandLine.EscapeMarkup()},
                            {"API", apiInfo.Name },
                            {"Method", string.Empty }
                        }));
                        Console.WriteLine();
                    }
                    if (apiInfo.Attribute?.HelpMethod is not null)
                    {
                        apiInfo.Attribute.RunHelpMethod(apiInfo, CliApi.CurrentContext);
                        Console.WriteLine();
                    }
                }
                AnsiConsole.MarkupLine($"[{CliApiInfo.HighlightColor}]{_("API methods")}:[/]");
                foreach (CliApiMethodInfo apiMethodInfo in apiInfo.Methods.Values)
                {
                    Console.WriteLine();
                    AnsiConsole.MarkupLine($"  [{CliApiInfo.ApiMethodNameColor}]{apiMethodInfo.Name}[/] {apiMethodInfo}");
                    Console.Write("  ");
                    if (apiMethodInfo == apiInfo.DefaultMethod || apiMethodInfo == apiInfo.Methods.Values.First())
                        AnsiConsole.Markup($"[{CliApiInfo.OptionalColor}]({_("Default")}) [/]");
                    AnsiConsole.MarkupLine($"[{CliApiInfo.RequiredColor}]{_(apiMethodInfo.Title.EscapeMarkup())}[/]");
                    if (apiMethodInfo.Description is not null) AnsiConsole.MarkupLine($"  {_(apiMethodInfo.Description.EscapeMarkup())}");
                    if (!Details || apiMethodInfo.Parameters.Count == 0) continue;
                    Console.WriteLine();
                    AnsiConsole.MarkupLine($"  {_("Arguments")}:");
                    foreach (CliApiArgumentInfo argInfo in apiMethodInfo.Parameters.Values)
                    {
                        Console.WriteLine();
                        AnsiConsole.MarkupLine($"    {argInfo}");
                        Console.Write("    ");
                        AnsiConsole.Markup(argInfo.IsRequired && argInfo.Type != CliArgumentTypes.Flag
                            ? $"[{CliApiInfo.RequiredColor}]({_("Required")}) [/]"
                            : $"[{CliApiInfo.OptionalColor}]({_("Optional")}) [/]"
                            );
                        AnsiConsole.MarkupLine($"[{CliApiInfo.RequiredColor}]{_(argInfo.Title.EscapeMarkup())}[/]");
                        if (argInfo.Description is not null) AnsiConsole.MarkupLine($"    {_(argInfo.Description.EscapeMarkup())}");
                    }
                }
                if (CliApi.ExportedApis.IsCliHelpApiServed())
                {
                    Console.WriteLine();
                    Console.WriteLine($"{_("To display detailed help for an API method")}:");
                    Console.WriteLine();
                    AnsiConsole.MarkupLine($"  [{CliApiInfo.RequiredColor}]{CliApi.CommandLine.EscapeMarkup()} help[/] --api [{CliApiInfo.ApiNameColor}]{ApiName}[/] --method [{CliApiInfo.ApiMethodNameColor}]{_("Method")}[/] [{CliApiInfo.DecorationColor}]([/][{CliApiInfo.OptionalColor}]-Details[/][{CliApiInfo.DecorationColor}])[/]");
                    if (!Details)
                    {
                        Console.WriteLine();
                        AnsiConsole.MarkupLine(_("The optional [%{0}]-Details[/] flag forces the help API to output more details.", CliApiInfo.OptionalColor.ToString()));
                    }
                }
            }
            else
            {
                AnsiConsole.Markup($"{_("Help for API method")} [{CliApiInfo.ApiNameColor}]{ApiName}[/] [{CliApiInfo.ApiMethodNameColor}]{ApiMethodName}[/]: [{CliApiInfo.DecorationColor}]{_(methodInfo.Title.EscapeMarkup())}[/]");
                if (methodInfo == apiInfo.DefaultMethod || methodInfo == apiInfo.Methods.Values.First())
                    AnsiConsole.Markup($"[{CliApiInfo.OptionalColor}] ({_("Default")})[/]");
                Console.WriteLine();
                if (methodInfo.Description is not null)
                {
                    Console.WriteLine();
                    AnsiConsole.MarkupLine(_(methodInfo.Description.EscapeMarkup()));
                }
                Console.WriteLine();
                AnsiConsole.MarkupLine($"[{CliApiInfo.HighlightColor}]{_("Usage")}:[/]");
                AnsiConsole.MarkupLine($"  {CliApi.ExportedApis.GetApiMethodSyntax(apiInfo.Name, methodInfo.Name, CliApi.CommandLine.EscapeMarkup())}");
                bool hasDetails = false;
                if (methodInfo.Parameters.Count != 0)
                {
                    Console.WriteLine();
                    AnsiConsole.MarkupLine($"[{CliApiInfo.HighlightColor}]{_("Arguments")}:[/]");
                    foreach (CliApiArgumentInfo argInfo in methodInfo.Parameters.Values)
                    {
                        Console.WriteLine();
                        AnsiConsole.MarkupLine($"  {argInfo}");
                        Console.Write("  ");
                        AnsiConsole.Markup(argInfo.IsRequired && argInfo.Type != CliArgumentTypes.Flag
                            ? $"[{CliApiInfo.RequiredColor}]({_("Required")}) [/]"
                            : $"[{CliApiInfo.OptionalColor}]({_("Optional")}) [/]"
                            );
                        AnsiConsole.MarkupLine($"[{CliApiInfo.RequiredColor}]{_(argInfo.Title.EscapeMarkup())}[/]");
                        if (argInfo.Description is not null) AnsiConsole.MarkupLine($"  {_(argInfo.Description.EscapeMarkup())}");
                        if (Details)
                        {
                            if (argInfo.Attribute?.HelpTextProperty is not null)
                            {
                                Console.WriteLine();
                                AnsiConsole.MarkupLine(argInfo.Attribute.GetHelpText()!.Parse(new Dictionary<string, string>()
                                {
                                    {"CommandLine", CliApi.CommandLine.EscapeMarkup()},
                                    {"API", apiInfo.Name },
                                    {"Method", methodInfo.Name }
                                }));
                            }
                            if (argInfo.Attribute?.HelpMethod is not null)
                            {
                                Console.WriteLine();
                                argInfo.Attribute.RunHelpMethod(argInfo, CliApi.CurrentContext);
                            }
                        }
                        else if (argInfo.Attribute?.HelpTextProperty is not null || argInfo.Attribute?.HelpMethod is not null)
                        {
                            hasDetails = true;
                        }
                    }
                }
                if (Details)
                {
                    if (methodInfo.ExitCodes.Count != 0)
                    {
                        Console.WriteLine();
                        AnsiConsole.MarkupLine($"[{CliApiInfo.HighlightColor}]{_("Exit codes")}:[/]");
                        foreach (ExitCodeInfo exitCode in methodInfo.ExitCodes.Values)
                            AnsiConsole.MarkupLine($"  [{CliApiInfo.RequiredColor}]{exitCode.Code}[/]: {_(exitCode.Description?.EscapeMarkup() ?? $"({_("undocumented")})")}");
                    }
                    if (methodInfo.StdIn is not null)
                    {
                        Console.WriteLine();
                        AnsiConsole.MarkupLine($"[{CliApiInfo.HighlightColor}]STDIN:[/]  {(methodInfo.StdIn.Required ? $"[{CliApiInfo.RequiredColor}]({_("Required")})" : $"[{CliApiInfo.OptionalColor}]({_("Optional")})")}[/] {_(methodInfo.StdIn.Description.EscapeMarkup())}");
                    }
                    if (methodInfo.StdOut is not null)
                    {
                        Console.WriteLine();
                        AnsiConsole.MarkupLine($"[{CliApiInfo.HighlightColor}]STDOUT:[/] {(methodInfo.StdOut.Required ? $"[{CliApiInfo.RequiredColor}]({_("Required")})" : $"[{CliApiInfo.OptionalColor}]({_("Optional")})")}[/] {_(methodInfo.StdOut.Description.EscapeMarkup())}");
                    }
                    if (methodInfo.StdErr is not null)
                    {
                        Console.WriteLine();
                        AnsiConsole.MarkupLine($"[{CliApiInfo.HighlightColor}]STDERR:[/] {_(methodInfo.StdErr.Description.EscapeMarkup())}");
                    }
                    if (methodInfo.Attribute?.HelpTextProperty is not null || methodInfo.Attribute?.HelpMethod is not null)
                    {
                        Console.WriteLine();
                        AnsiConsole.MarkupLine($"[{CliApiInfo.HighlightColor}]{_("Usage details")}:[/]");
                        if (methodInfo.Attribute.HelpTextProperty is not null)
                        {
                            Console.WriteLine();
                            AnsiConsole.MarkupLine(methodInfo.Attribute.GetHelpText()!.Parse(new Dictionary<string, string>()
                            {
                                {"CommandLine", CliApi.CommandLine.EscapeMarkup()},
                                {"API", apiInfo.Name },
                                {"Method", methodInfo.Name }
                            }));
                        }
                        if (methodInfo.Attribute.HelpMethod is not null)
                        {
                            methodInfo.Attribute.RunHelpMethod(methodInfo, CliApi.CurrentContext);
                            Console.WriteLine();
                        }
                    }
                }
                else if (
                    (hasDetails || methodInfo.ExitCodes.Count != 0 || methodInfo.StdIn is not null || methodInfo.StdOut is not null || methodInfo.Attribute?.HelpTextProperty is not null) &&
                    CliApi.ExportedApis.IsCliHelpApiServed()
                    )
                {
                    Console.WriteLine();
                    Console.WriteLine($"{_("To display detailed help for this API method")}:");
                    Console.WriteLine();
                    AnsiConsole.MarkupLine($"  [{CliApiInfo.RequiredColor}]{CliApi.CommandLine.EscapeMarkup()} help[/] --api [{CliApiInfo.ApiNameColor}]{ApiName}[/] --method [{CliApiInfo.ApiMethodNameColor}]{ApiMethodName}[/] [{CliApiInfo.RequiredColor}]-Details[/]");
                }
            }
            return 0;
        }

        /// <summary>
        /// Display context help
        /// </summary>
        /// <param name="context">CLI API context</param>
        /// <returns>Exit code</returns>
        protected virtual int DisplayHelp(CliApiContext context)
        {
            if (CliApi.CurrentContext == context)
            {
                ApiName = context.API?.GetType().GetCliApiName();
                ApiMethodName = context.Method?.GetCliApiMethodName();
                Help();
            }
            else
            {
                CliApiHelper.Default.DisplayHelp(context);
            }
            return 1;
        }

        /// <inheritdoc/>
        int ICliApiHelpProvider.DisplayHelp(CliApiContext context) => DisplayHelp(context);

        /// <inheritdoc/>
        int ICliApiHelper.DisplayHelp(CliApiContext context) => DisplayHelp(context);

        /// <inheritdoc/>
        Task<int> ICliApiErrorHandler.HandleApiErrorAsync(CliApiContext context) => Task.FromResult(DisplayHelp(context));

        /// <summary>
        /// Delegate for a help details handler
        /// </summary>
        /// <param name="apiElement">API element (<see cref="CliApiInfo"/>, <see cref="CliApiMethodInfo"/>, <see cref="CliApiArgumentInfo"/>)</param>
        /// <param name="context">Context</param>
        public delegate void DetailHelp_Delegate(object apiElement, CliApiContext context);
    }
}
