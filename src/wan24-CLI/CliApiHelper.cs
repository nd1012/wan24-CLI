using Spectre.Console;
using System.Diagnostics.Contracts;
using System.Reflection;

namespace wan24.CLI
{
    /// <summary>
    /// CLI API default helper
    /// </summary>
    public class CliApiHelper : ICliApiHelper
    {
        /// <summary>
        /// Default instance
        /// </summary>
        internal static readonly CliApiHelper Default = new();

        /// <summary>
        /// Constructor
        /// </summary>
        public CliApiHelper() { }

        /// <inheritdoc/>
        public virtual int DisplayHelp(CliApiContext context)
        {
            Contract.Assume(CliApi.ExportedApis is not null);
            CliArgException? argException = context.Exception as CliArgException;
            if (context.Exception is not null)
            {
                AnsiConsole.MarkupLine(argException is not null
                    ? $"[white on red]Invalid arguments: [{context.Exception.Source}] {context.Exception.Message}[/]"
                    : $"[white on red]An exception has been catched: {context.Exception.ToString().EscapeMarkup()}[/]");
                Console.WriteLine();
            }
            string? apiName = context.API?.GetType().GetCliApiName();
            CliApiInfo? apiInfo = apiName is null ? null : CliApi.ExportedApis[apiName];
            string? methodName = context.Method?.GetCliApiMethodName();
            CliApiMethodInfo? methodInfo = methodName is null ? null : apiInfo!.Methods[methodName];
            if (apiInfo is null)
            {
                Console.WriteLine("General usage:");
                Console.WriteLine();
                AnsiConsole.MarkupLine($"\tdotnet {Path.GetFileName(Assembly.GetEntryAssembly()!.Location)} [{CliApiInfo.ApiNameColor}]API[/] [{CliApiInfo.DecorationColor}]([/][{CliApiInfo.ApiMethodNameColor}]Method[/][{CliApiInfo.DecorationColor}]) ([/][{CliApiInfo.OptionalColor}]Arguments[/][{CliApiInfo.DecorationColor}])[/]");
                Console.WriteLine();
                Console.WriteLine("Available APIs:");
                Console.WriteLine();
                NullabilityInfoContext nic = new();
                foreach (Type api in context.ExportedApis)
                {
                    apiInfo = new(api, nic);
                    AnsiConsole.MarkupLine($"\t[{CliApiInfo.ApiNameColor}]{apiInfo.Name}[/]: [{CliApiInfo.DecorationColor}]{apiInfo.Title}[/]");
                    if (apiInfo.Description is not null) AnsiConsole.MarkupLine($"\t{apiInfo.Description}");
                    Console.WriteLine();
                }
                Console.WriteLine("To display detailed help for an API or an API method:");
                Console.WriteLine();
                AnsiConsole.MarkupLine($"\t[{CliApiInfo.RequiredColor}]dotnet {Path.GetFileName(Assembly.GetEntryAssembly()!.Location)} help[/] --api [{CliApiInfo.ApiNameColor}]API[/] [{CliApiInfo.DecorationColor}]([/]--method [{CliApiInfo.ApiMethodNameColor}]Method[/][{CliApiInfo.DecorationColor}])[/]");
            }
            else if (methodInfo is null)
            {
                AnsiConsole.MarkupLine($"[{CliApiInfo.ApiNameColor}]{apiInfo.Name}[/]: [{CliApiInfo.DecorationColor}]{apiInfo.Title}[/]");
                if (apiInfo.Description is not null) Console.WriteLine(apiInfo.Description);
                Console.WriteLine();
                Console.WriteLine("General usage:");
                Console.WriteLine();
                AnsiConsole.MarkupLine($"\tdotnet {Path.GetFileName(Assembly.GetEntryAssembly()!.Location)} [{CliApiInfo.ApiNameColor}]{apiInfo.Name}[/] [{CliApiInfo.DecorationColor}]([/][{CliApiInfo.ApiMethodNameColor}]Method[/][{CliApiInfo.DecorationColor}]) ([/][{CliApiInfo.OptionalColor}]Arguments[/][{CliApiInfo.DecorationColor}])[/]");
                Console.WriteLine();
                Console.WriteLine("Available API methods:");
                Console.WriteLine();
                foreach (CliApiMethodInfo apiMethodInfo in apiInfo.Methods.Values)
                {
                    AnsiConsole.MarkupLine($"\t[{CliApiInfo.ApiMethodNameColor}]{apiMethodInfo.Name}[/] {apiMethodInfo}");
                    AnsiConsole.MarkupLine($"\t[{CliApiInfo.DecorationColor}]{apiMethodInfo.Title}[/]");
                    if (apiMethodInfo.Description is not null) Console.WriteLine($"\t{apiMethodInfo.Description}");
                    Console.WriteLine();
                }
                Console.WriteLine("To display detailed help for an API method:");
                Console.WriteLine();
                AnsiConsole.MarkupLine($"\t[{CliApiInfo.RequiredColor}]dotnet {Path.GetFileName(Assembly.GetEntryAssembly()!.Location)} help[/] --api [{CliApiInfo.ApiNameColor}]{apiInfo.Name}[/] --method [{CliApiInfo.ApiMethodNameColor}]Method[/]");
            }
            else
            {
                AnsiConsole.MarkupLine($"[{CliApiInfo.ApiNameColor}]{apiInfo.Name}[/] [{CliApiInfo.ApiMethodNameColor}]{methodInfo.Name}[/]: [{CliApiInfo.DecorationColor}]{methodInfo.Title}[/]");
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
            }
            return 1;
        }
    }
}
