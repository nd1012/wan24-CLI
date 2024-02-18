using Spectre.Console;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Reflection;
using wan24.Core;
using wan24.ObjectValidation;

namespace wan24.CLI
{
    /// <summary>
    /// CLI help API
    /// </summary>
    [CliApi("help")]
    [DisplayText("CLI help API")]
    [Description("Provides generated general usage instructions for a CLI API")]
    public class CliHelpApi : ICliApiHelpProvider
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
        [Description("Add the API name to the command to specify the API to use and display method informations")]
        public string? ApiName { get; set; }

        /// <summary>
        /// API method name
        /// </summary>
        [CliApi("method")]
        [DisplayText("Name of the API method to display help for")]
        [Description("Add the API method name to the command to specify the API method to use and display argument informations")]
        public string? ApiMethodName { get; set; }

        /// <summary>
        /// Print details, if available?
        /// </summary>
        [CliApi]
        [DisplayText("Print help details, if available")]
        [Description("Some APIs/methods/arguments may provide detailed usage informations on request")]
        public bool Details { get; set; }

        /// <summary>
        /// Help
        /// </summary>
        /// <returns>Exit code <c>0</c></returns>
        [CliApi(IsDefault = true)]
        [DisplayText("Display CLI API help")]
        [Description("Displays context help based on the given arguments (without any following arguments available APIs are being listed)")]
        public virtual int Help()
        {
            Contract.Assume(CliApi.CurrentContext is not null);
            Contract.Assume(CliApi.ExportedApis is not null);
            CliApiInfo? apiInfo = ApiName is null
                ? null
                : CliApi.ExportedApis.Values.FirstOrDefault(a => a.Name.Equals(ApiName, StringComparison.OrdinalIgnoreCase));
            CliApiMethodInfo? methodInfo = apiInfo is null || ApiMethodName is null
                ? null
                : apiInfo.Methods.Values.FirstOrDefault(m => m.Name.Equals(ApiMethodName, StringComparison.OrdinalIgnoreCase));
            if(ApiName is null || apiInfo is null)
            {
                if (ApiName is not null)
                {
                    AnsiConsole.MarkupLine($"[white on red]API not found: \"{ApiName}\"[/]");
                    Console.WriteLine();
                }
                Console.WriteLine("General usage:");
                Console.WriteLine();
                AnsiConsole.MarkupLine($"\tdotnet {Path.GetFileName(Assembly.GetEntryAssembly()!.Location)} [{CliApiInfo.ApiNameColor}]API[/] [{CliApiInfo.DecorationColor}]([/][{CliApiInfo.ApiMethodNameColor}]Method[/][{CliApiInfo.DecorationColor}]) ([/][{CliApiInfo.OptionalColor}]Arguments[/][{CliApiInfo.DecorationColor}])[/]");
                Console.WriteLine();
                Console.WriteLine("Available APIs:");
                Console.WriteLine();
                NullabilityInfoContext nic = new();
                foreach (Type api in CliApi.CurrentContext.ExportedApis)
                {
                    apiInfo = new(api, nic);
                    AnsiConsole.MarkupLine($"\t[{CliApiInfo.ApiNameColor}]{apiInfo.Name}[/]: [{CliApiInfo.DecorationColor}]{apiInfo.Title}[/]");
                    if (apiInfo.Description is not null) AnsiConsole.MarkupLine($"\t{apiInfo.Description}");
                    Console.WriteLine();
                    if (!Details) continue;
                    Console.WriteLine("\tAvailable API methods:");
                    Console.WriteLine();
                    foreach (CliApiMethodInfo apiMethodInfo in apiInfo.Methods.Values)
                    {
                        AnsiConsole.MarkupLine($"\t\t[{CliApiInfo.ApiMethodNameColor}]{apiMethodInfo.Name}[/] {apiMethodInfo}");
                        AnsiConsole.MarkupLine($"\t\t[{CliApiInfo.DecorationColor}]{apiMethodInfo.Title}[/]");
                        if (apiMethodInfo.Description is not null) Console.WriteLine($"\t\t{apiMethodInfo.Description}");
                        Console.WriteLine();
                    }
                }
                Console.WriteLine("To display detailed help for an API or an API method:");
                Console.WriteLine();
                AnsiConsole.MarkupLine($"\t[{CliApiInfo.RequiredColor}]dotnet {Path.GetFileName(Assembly.GetEntryAssembly()!.Location)} help[/] --api [{CliApiInfo.ApiNameColor}]API[/] [{CliApiInfo.DecorationColor}]([/]--method [{CliApiInfo.ApiMethodNameColor}]Method[/][{CliApiInfo.DecorationColor}]) (-Details)[/]");
                Console.WriteLine();
                AnsiConsole.MarkupLine($"The optional [{CliApiInfo.DecorationColor}]-Details[/] flag forces the help API to output more details.");
            }
            else if(ApiMethodName is null || methodInfo is null)
            {
                if (ApiMethodName is not null)
                {
                    AnsiConsole.MarkupLine($"[white on red]API method not found: \"{ApiMethodName}\"[/]");
                    Console.WriteLine();
                }
                AnsiConsole.MarkupLine($"[{CliApiInfo.ApiNameColor}]{ApiName}[/]: [{CliApiInfo.DecorationColor}]{apiInfo.Title}[/]");
                if (apiInfo.Description is not null) Console.WriteLine(apiInfo.Description);
                Console.WriteLine();
                Console.WriteLine("General usage:");
                Console.WriteLine();
                AnsiConsole.MarkupLine($"\tdotnet {Path.GetFileName(Assembly.GetEntryAssembly()!.Location)} [{CliApiInfo.ApiNameColor}]{ApiName}[/] [{CliApiInfo.DecorationColor}]([/][{CliApiInfo.ApiMethodNameColor}]Method[/][{CliApiInfo.DecorationColor}]) ([/][{CliApiInfo.OptionalColor}]Arguments[/][{CliApiInfo.DecorationColor}])[/]");
                Console.WriteLine();
                Console.WriteLine("Available API methods:");
                Console.WriteLine();
                foreach (CliApiMethodInfo apiMethodInfo in apiInfo.Methods.Values)
                {
                    AnsiConsole.MarkupLine($"\t[{CliApiInfo.ApiMethodNameColor}]{apiMethodInfo.Name}[/] {apiMethodInfo}");
                    AnsiConsole.MarkupLine($"\t[{CliApiInfo.DecorationColor}]{apiMethodInfo.Title}[/]");
                    if (apiMethodInfo.Description is not null) Console.WriteLine($"\t{apiMethodInfo.Description}");
                    Console.WriteLine();
                    if (!Details || apiMethodInfo.Parameters.Count == 0) continue;
                    Console.WriteLine("\tAvailable method arguments:");
                    Console.WriteLine();
                    foreach (CliApiArgumentInfo argInfo in apiMethodInfo.Parameters.Values)
                    {
                        AnsiConsole.MarkupLine($"\t\t{argInfo}");
                        Console.Write("\t\t");
                        if (argInfo.IsRequired) AnsiConsole.Markup($"[{CliApiInfo.RequiredColor}](Required) [/]");
                        else AnsiConsole.Markup($"[{CliApiInfo.OptionalColor}](Optional) [/]");
                        AnsiConsole.MarkupLine($"[{CliApiInfo.DecorationColor}]{argInfo.Title}[/]");
                        if (argInfo.Description is not null) Console.WriteLine($"\t\t{argInfo.Description}");
                        Console.WriteLine();
                    }
                }
                Console.WriteLine("To display detailed help for an API method:");
                Console.WriteLine();
                AnsiConsole.MarkupLine($"\t[{CliApiInfo.RequiredColor}]dotnet {Path.GetFileName(Assembly.GetEntryAssembly()!.Location)} help[/] --api [{CliApiInfo.ApiNameColor}]{ApiName}[/] --method [{CliApiInfo.ApiMethodNameColor}]Method[/]");
            }
            else
            {
                AnsiConsole.MarkupLine($"[{CliApiInfo.ApiNameColor}]{ApiName}[/] [{CliApiInfo.ApiMethodNameColor}]{ApiMethodName}[/]: [{CliApiInfo.DecorationColor}]{methodInfo.Title}[/]");
                if (methodInfo.Description is not null) Console.WriteLine(methodInfo.Description);
                Console.WriteLine();
                Console.WriteLine("General usage:");
                Console.WriteLine();
                AnsiConsole.MarkupLine($"\t{CliApi.ExportedApis.GetApiMethodSyntax(apiInfo.Name, methodInfo.Name, $"dotnet {Path.GetFileName(Assembly.GetEntryAssembly()!.Location)}")}");
                Console.WriteLine();
                if (methodInfo.Parameters.Count != 0)
                {
                    Console.WriteLine("Available method arguments:");
                    Console.WriteLine();
                    foreach (CliApiArgumentInfo argInfo in methodInfo.Parameters.Values)
                    {
                        AnsiConsole.MarkupLine($"\t{argInfo}");
                        Console.Write("\t");
                        if (argInfo.IsRequired) AnsiConsole.Markup($"[{CliApiInfo.RequiredColor}](Required) [/]");
                        else AnsiConsole.Markup($"[{CliApiInfo.OptionalColor}](Optional) [/]");
                        AnsiConsole.MarkupLine($"[{CliApiInfo.DecorationColor}]{argInfo.Title}[/]");
                        if (argInfo.Description is not null) Console.WriteLine($"\t{argInfo.Description}");
                        Console.WriteLine();
                    }
                }
                if (Details && methodInfo.ExitCodes.Count != 0)
                {
                    Console.WriteLine("Used exit codes:");
                    Console.WriteLine();
                    foreach(ExitCodeAttribute exitCode in methodInfo.ExitCodes.Values)
                        AnsiConsole.MarkupLine($"\t[{CliApiInfo.RequiredColor}]{exitCode.Code}[/]: {exitCode.Description ?? "(documentation missing)"}");
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
            CliApiHelper.Default.DisplayHelp(context);
            return 1;
        }

        /// <inheritdoc/>
        int ICliApiHelpProvider.DisplayHelp(CliApiContext context) => DisplayHelp(context);
    }
}
