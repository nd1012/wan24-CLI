using Spectre.Console;
using System.Collections.Frozen;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using wan24.Core;
using wan24.ObjectValidation;
using static wan24.Core.Logger;
using static wan24.Core.Logging;
using static wan24.Core.TranslationHelper;
using static wan24.Core.TranslationHelper.Ext;

namespace wan24.CLI
{
    /// <summary>
    /// CLI API helper
    /// </summary>
    public static partial class CliApi
    {
        /// <summary>
        /// STDERR ANSI console
        /// </summary>
        public static IAnsiConsole StdErr { get; } = AnsiConsole.Create(new AnsiConsoleSettings()
        {
            Out = new AnsiConsoleOutput(Console.Error)
        });

        /// <summary>
        /// Helper (may implement <see cref="ICliApiHelpProvider"/> and/or <see cref="ICliApiErrorHandler"/>, too)
        /// </summary>
        public static ICliApiHelper Helper { get; set; } = CliApiHelper.Default;

        /// <summary>
        /// Use <c>InvokeAuth</c> for invoking API methods?
        /// </summary>
        public static bool UseInvokeAuto { get; set; }

        /// <summary>
        /// Current CLI API context
        /// </summary>
        public static CliApiContext? CurrentContext { get; internal set; }

        /// <summary>
        /// Exported APIs of the current run
        /// </summary>
        public static FrozenDictionary<string, CliApiInfo>? ExportedApis { get; internal set; }

        /// <summary>
        /// General header message (Spectre.Console markup is supported)
        /// </summary>
        public static string? GeneralHeader { get; set; }

        /// <summary>
        /// Has the general header message been displayed?
        /// </summary>
        public static bool GeneralHeaderDisplayed { get; internal set; }

        /// <summary>
        /// Help header message (Spectre.Console markup is supported)
        /// </summary>
        public static string? HelpHeader { get; set; }

        /// <summary>
        /// Has the help header message been displayed?
        /// </summary>
        public static bool HelpHeaderDisplayed { get; internal set; }

        /// <summary>
        /// The app command line to use
        /// </summary>
        public static string CommandLine { get; set; } = ENV.App.EndsWith(".dll", StringComparison.OrdinalIgnoreCase)
            ? $"dotnet {Path.GetFileName(ENV.App)}"
            : Path.GetFileName(ENV.App);

        /// <summary>
        /// Custom argument type parser delegates
        /// </summary>
        public static Dictionary<Type, ParseType_Delegate> CustomArgumentParsers { get; } = [];

        /// <summary>
        /// Display full exceptions in error messages?
        /// </summary>
        [CliConfig]
        public static bool DisplayFullExceptions { get; set; }

        /// <summary>
        /// State
        /// </summary>
        public static IEnumerable<Core.Status> State
        {
            get
            {
                // General
                yield return new(__("Command"), CommandLine, __("The used command line"));
                yield return new(__("Helper"), Helper.GetType(), __("The used API helper CLR type"));
                yield return new(__("CLI API running"), CurrentContext is not null, __("If a CLI API is being executed at present"));
                yield return new(__("Invoke auto"), UseInvokeAuto, __("If using auto-invokation"));
                yield return new(__("Full exceptions"), DisplayFullExceptions, __("If displaying full exception informations on error"));
                // Current context
                if (CurrentContext is not null)
                {
                    yield return new(__("API"), CurrentContext.API?.GetType(), __("CLR type of the executing API"));
                    yield return new(__("Arguments"), CurrentContext.Arguments?.ToString(), __("Used CLI arguments"));
                    yield return new(__("Method"), CurrentContext.Method?.Name, __("CLR name of the executing API method"));
                    yield return new(__("Exception"), CurrentContext.Exception?.Message ?? CurrentContext.Exception?.GetType().ToString(), __("Exception during handling the API call"));
                    foreach (Type type in CurrentContext.ExportedApis)
                        yield return new(__("Type"), type, __("CLR type of an exported API"), __("Exported APIs"));
                }
                // Custom parsers
                yield return new(__("Custom parsers"), CustomArgumentParsers.Count, __("The number of custom CLI argument type parsers"), __("Custom parsers"));
                foreach (Type type in CustomArgumentParsers.Keys)
                    yield return new(__("Type"), type, __("Custom parsed CLI argument CLR type"), __("Custom parsers"));
                // Color profiles
                yield return new(__("Color profiles"), ConsoleColorProfile.Registered.Count, __("The number of available console color profiles"), __("Color profiles"));
                foreach (string profile in ConsoleColorProfile.Registered.Keys)
                    yield return new(__("Profile"), profile, __("Name of a custom console color profile"), __("Color profiles"));
                // Used colors
                yield return new(__("Background"), CliApiInfo.BackGroundColor, __("The used background color name"), __("Colors"));
                yield return new(__("Highlight"), CliApiInfo.HighlightColor, __("The used highlight color name"), __("Colors"));
                yield return new(__("Required"), CliApiInfo.RequiredColor, __("The used required element color name"), __("Colors"));
                yield return new(__("Optional"), CliApiInfo.OptionalColor, __("The used optional element color name"), __("Colors"));
                yield return new(__("Decoration"), CliApiInfo.DecorationColor, __("The used decoration color name"), __("Colors"));
                yield return new(__("API"), CliApiInfo.ApiNameColor, __("The used API name color name"), __("Colors"));
                yield return new(__("Method"), CliApiInfo.ApiMethodNameColor, __("The used API method name color name"), __("Colors"));
            }
        }

        /// <summary>
        /// Run the CLI API
        /// </summary>
        /// <param name="args">Argument list</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <param name="exportedApis">Exported APIs</param>
        /// <returns>Exit code</returns>
        public static async Task<int> RunAsync(string[]? args = null, CancellationToken cancellationToken = default, params Type[] exportedApis)
        {
            await Task.Yield();
            cancellationToken.ThrowIfCancellationRequested();
            if(args is null)
            {
                args = ENV.CliArguments;
                if (args.Length > 0) args = [.. args.AsSpan(1)];
            }
            // Parse CLI arguments
            CliArguments ca;
            try
            {
                ca = (CliArguments)args;
            }
            catch (Exception ex)
            {
                WriteError($"Parsing CLI arguments failed: {ex}");
                return await DisplayHelpAsync(new()
                {
                    ExportedApis = exportedApis,
                    Exception = ex
                }).DynamicContext();
            }
            return await RunAsync(ca, args, cancellationToken, exportedApis).DynamicContext();
        }

        /// <summary>
        /// Run multiple CLI API calls (chunk arguments for API calls using a dash <c>-</c>; any parsing error or exit code <c>!=0</c> will cancel the execution loop; a call without 
        /// any argument isn't supported; calls will be processed sequential)
        /// </summary>
        /// <param name="args">Argument list</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <param name="exportedApis">Exported APIs</param>
        /// <returns>Exit code</returns>
        public static async Task<int> RunMultiAsync(string[]? args = null, CancellationToken cancellationToken = default, params Type[] exportedApis)
        {
            await Task.Yield();
            cancellationToken.ThrowIfCancellationRequested();
            if (args is null)
            {
                args = ENV.CliArguments;
                if (args.Length > 0) args = [.. args.AsSpan(1)];
            }
            // Find exported APIs
            if (exportedApis.Length == 0)
            {
                if (Trace) WriteTrace("Find exported APIs");
                exportedApis = [.. TypeHelper.Instance.Assemblies.FindExportedCliApis().Select(a => a.Type)];
                if (exportedApis.Length == 0 || (exportedApis.Length == 1 && typeof(CliHelpApi).IsAssignableFrom(exportedApis[0])))
                    throw new InvalidProgramException("No CLI APIs exported");
            }
            ExportedApis = CliApiInfo.Create(exportedApis);
            try
            {
                // Display help
                if (args.Length == 0)
                {
                    if (Trace) WriteTrace("No arguments given - display help");
                    return await DisplayHelpAsync(new()
                    {
                        ExportedApis = exportedApis
                    }).DynamicContext();
                }
                // Process argument chunks
                int startArg = 0,
                    endArg = 0;
                bool isValue = false;
                for (int i = 0, exitCode; i < args.Length; endArg++, i++)
                    if (!isValue && args[i].Length == 1 && args[i][0] == '-')
                    {
                        if (Trace) WriteTrace("Found API call delimiter");
                        if (startArg == endArg)
                        {
                            if (Trace) WriteTrace("No arguments found - can't execute any API call");
                            continue;
                        }
                        exitCode = await RunAsync(args.AsSpan(startArg, endArg - startArg).ToArray(), cancellationToken, exportedApis).DynamicContext();
                        if (Trace) WriteTrace($"API call returned with exit code {exitCode}");
                        if (exitCode != 0)
                        {
                            AnsiConsole.MarkupLine($"[{CliApiInfo.ApiNameColor}{(CliApiInfo.ApiNameColor.Contains(" on ") ? string.Empty : $" {CliApiInfo.BackGroundColor}")}]{_("Break in arguments chunk ${0} with exit code %{1}", (i + 1).ToString(), exitCode.ToString())}[/]");
                            return exitCode;
                        }
                        startArg = endArg + 1;
                        isValue = false;
                    }
                    else
                    {
                        isValue = args[i].Length >= 2 && args[i][0] == '-' && args[i][1] == '-' && args[i].Length != 2;
                    }
                return startArg == endArg
                    ? 0
                    : await RunAsync(args.AsSpan(startArg, endArg - startArg).ToArray(), cancellationToken, exportedApis).DynamicContext();
            }
            finally
            {
                ExportedApis = null;
            }
        }

        /// <summary>
        /// Run the CLI API
        /// </summary>
        /// <param name="ca">CLI arguments</param>
        /// <param name="args">Argument list</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <param name="exportedApis">Exported APIs</param>
        /// <returns>Exit code</returns>
        public static async Task<int> RunAsync(CliArguments ca, string[]? args = null, CancellationToken cancellationToken = default, params Type[] exportedApis)
        {
            await Task.Yield();
            DisplayGeneralHeader();
            cancellationToken.ThrowIfCancellationRequested();
            if (!Bootstrap.DidBoot) await Bootstrap.Async(cancellationToken: cancellationToken).DynamicContext();
            if (args is null)
            {
                args = ENV.CliArguments;
                if (args.Length > 0) args = [.. args.AsSpan(1)];
            }
            // Find exported APIs
            if (exportedApis.Length == 0)
            {
                if (Trace) WriteTrace("Find exported APIs");
                exportedApis = [.. TypeHelper.Instance.Assemblies.FindExportedCliApis().Select(a => a.Type)];
                if (exportedApis.Length == 0 || (exportedApis.Length == 1 && typeof(CliHelpApi).IsAssignableFrom(exportedApis[0])))
                    throw new InvalidProgramException("No CLI APIs exported");
            }
            CurrentContext = new()
            {
                ExportedApis = exportedApis,
                Arguments = ca
            };
            bool removeExportedApis = ExportedApis is null;
            if (removeExportedApis) ExportedApis = CliApiInfo.Create(exportedApis);
            try
            {
                // Find the CLI API to use
                int keyLessOffset = 0,
                    keyLessArgOffset = 0;
                object api;
                if (exportedApis.Length == 1)
                {
                    // Use the first and only API
                    if (Trace) WriteTrace("Only one API is exported");
                    api = exportedApis[0].ConstructAuto() ?? throw new InvalidProgramException($"Failed to instance CLI API {exportedApis[0]}");
                }
                else if (ca.KeyLessArguments.Count == 0)
                {
                    // Use the default API
                    Type type = FindDefaultApi(exportedApis) ?? exportedApis[0];
                    if (Trace) WriteTrace($"Using default API {type}");
                    api = type.ConstructAuto() ?? throw new InvalidProgramException($"Failed to instance CLI API {type}");
                    if (api is null)
                    {
                        if (Trace) WriteTrace($"Failed to instance default API {type}");
                        return await DisplayHelpAsync(new()
                        {
                            ExportedApis = exportedApis,
                            Arguments = ca
                        }).DynamicContext();
                    }
                }
                else
                {
                    // Find the named API
                    if (ca.KeyLessArguments[0] != args[0])
                    {
                        if (Trace) WriteTrace("API name not given");
                        return await DisplayHelpAsync(new()
                        {
                            ExportedApis = exportedApis,
                            Arguments = ca
                        }).DynamicContext();
                    }
                    string apiName = ca.KeyLessArguments[0].ToLower();
                    keyLessOffset = 1;
                    api = null!;
                    foreach (Type type in exportedApis)
                    {
                        if (type.GetCustomAttributeCached<CliApiAttribute>() is CliApiAttribute attr)
                        {
                            if (!attr.Name.Equals(apiName, StringComparison.CurrentCultureIgnoreCase)) continue;
                            if (Trace) WriteTrace($"Using named API {type} by attribute");
                        }
                        else if (!type.Name.Equals(apiName, StringComparison.CurrentCultureIgnoreCase))
                        {
                            continue;
                        }
                        else if (Trace)
                        {
                            WriteTrace($"Using named API {type} by class name");
                        }
                        api = type.ConstructAuto() ?? throw new InvalidProgramException($"Failed to instance CLI API {type}");
                        break;
                    }
                    if (api is null)
                    {
                        if (Trace) WriteTrace($"Named API \"{apiName}\" not found");
                        return await DisplayHelpAsync(new()
                        {
                            ExportedApis = exportedApis,
                            Arguments = ca
                        }).DynamicContext();
                    }
                    CurrentContext.API = api;
                }
                // Validate the required arguments
                try
                {
                    MapArguments(api, ca, keyLessOffset, ref keyLessArgOffset);
                }
                catch (Exception ex)
                {
                    WriteError($"Argument mapping failed with an exception: {ex}");
                    return await DisplayHelpAsync(new()
                    {
                        ExportedApis = exportedApis,
                        API = api,
                        Arguments = ca,
                        Exception = ex
                    }).DynamicContext();
                }
                keyLessOffset += keyLessArgOffset;
                keyLessArgOffset = 0;
                // Find the API method to call
                MethodInfo[] methods = FindApiMethods(api.GetType()).ToArray();
                MethodInfo mi;
                if (exportedApis.Length == 1 && methods.Length == 1)
                {
                    // Use the only API method of the only API
                    if (Trace) WriteTrace("Only one API method is exported");
                    mi = methods[0];
                }
                else if (ca.KeyLessArguments.Count == keyLessOffset || ca.KeyLessArguments[keyLessOffset] != args[keyLessOffset])
                {
                    // Use the default method
                    mi = FindDefaultApiMethod(methods)!;
                    if (mi is null)
                    {
                        if (Trace) WriteTrace("No default API method found");
                        return await DisplayHelpAsync(new()
                        {
                            ExportedApis = exportedApis,
                            API = api,
                            Arguments = ca
                        }).DynamicContext();
                    }
                    if (Trace) WriteTrace($"Using default API method {mi.Name}");
                }
                else
                {
                    // Find the named method
                    string methodName = ca.KeyLessArguments[keyLessOffset].ToLower();
                    keyLessOffset++;
                    mi = null!;
                    CliApiAttribute attr;
                    foreach (MethodInfo m in methods)
                    {
                        attr = m.GetCustomAttributeCached<CliApiAttribute>()!;
                        if (
                            (attr.Name != string.Empty && !attr.Name.Equals(methodName, StringComparison.CurrentCultureIgnoreCase)) ||
                            (attr.Name == string.Empty && !m.Name.Equals(methodName, StringComparison.CurrentCultureIgnoreCase))
                            )
                            continue;
                        mi = m;
                        break;
                    }
                    if (mi is null)
                    {
                        if (Trace) WriteTrace($"Named method \"{methodName}\" not found");
                        return await DisplayHelpAsync(new()
                        {
                            ExportedApis = exportedApis,
                            API = api,
                            Arguments = ca
                        }).DynamicContext();
                    }
                    if (Trace) WriteTrace($"Using named method {mi.Name}");
                }
                CurrentContext.Method = mi;
                // Collect API method invokation parameters
                List<object?> param = [];
                if (UseInvokeAuto) param.Add(cancellationToken);
                try
                {
                    object? value;
                    IEnumerable<System.ComponentModel.DataAnnotations.ValidationResult> results;
                    foreach (ParameterInfo pi in mi.GetParameters())
                        if (pi.GetCustomAttributeCached<CliApiAttribute>() is not CliApiAttribute attr)
                        {
                            // Not a CLI argument parameter
                            if (UseInvokeAuto) continue;
                            param.Add(pi.HasDefaultValue ? pi.DefaultValue : pi.ParameterType.ConstructAuto());
                        }
                        else if (typeof(ICliArguments).IsAssignableFrom(pi.ParameterType))
                        {
                            // Object which hosts arguments
                            if (!pi.ParameterType.CanConstruct() || pi.ParameterType == typeof(object))
                                throw new InvalidProgramException($"{api.GetType()}.{mi.Name}[{pi.Name}] parameter value type {pi.ParameterType} must not be abstract");
                            value = (pi.HasDefaultValue && pi.DefaultValue is not null ? pi.DefaultValue : pi.ParameterType.ConstructAuto())
                                ?? throw new InvalidProgramException($"Failed to instance {api.GetType()}.{mi.Name}[{pi.Name}] parameter value type {pi.ParameterType}");
                            if (Trace) WriteTrace($"Mapping arguments to hosting object {value.GetType()}");
                            MapArguments(value, ca, keyLessOffset, ref keyLessArgOffset);
                            param.Add(value);
                        }
                        else
                        {
                            // Parsed argument
                            if (Trace) WriteTrace($"Using parsed argument {pi.Name} ({pi.ParameterType})");
                            value = ParseArgument(pi.Name!, pi.ParameterType, ca, attr, keyLessOffset, ref keyLessArgOffset).Value;
                            results = value.ValidateValue(pi.GetCustomAttributesCached<ValidationAttribute>());
                            if (results.Any())
                                throw new CliArgException($"Parsed argument validation failed: {results.First().ErrorMessage}", pi.Name, new ObjectValidationException(results));
                            param.Add(value);
                        }
                }
                catch (Exception ex)
                {
                    WriteError($"Invokation parameter preparation failed with an exception: {ex}");
                    return await DisplayHelpAsync(new()
                    {
                        ExportedApis = exportedApis,
                        API = api,
                        Method = mi,
                        Arguments = ca,
                        Exception = ex
                    }).DynamicContext();
                }
                CurrentContext.Parameters = [.. param];
                // Invoke the API method and handle the result
                try
                {
                    if (UseInvokeAuto)
                    {
                        int res = (typeof(Task).IsAssignableFrom(mi.ReturnType)
                            ? await mi.InvokeAutoAsync(mi.IsStatic ? null : api, [.. param]).DynamicContext()
                            : mi.InvokeAuto(mi.IsStatic ? null : api, [.. param])) is int exitCode ? exitCode : 0;
                        if (Trace) WriteTrace($"API method returned exit code {res}");
                        return res;
                    }
                    object? ret = mi.Invoke(mi.IsStatic ? null : api, [.. param]);
                    if (ret is not null)
                    {
                        if (ret is int res)
                        {
                            if (Trace) WriteTrace($"API method returned exit code {res}");
                            return res;
                        }
                        if (ret is Task task)
                        {
                            await task.DynamicContext();
                            if (mi.ReturnType.IsGenericType && task.GetResult(mi.ReturnType.GetGenericArguments()[0]) is int exitCode)
                            {
                                if (Trace) WriteTrace($"API method returned exit code {exitCode}");
                                return exitCode;
                            }
                        }
                        else if (ret is ValueTask valueTask)
                        {
                            await valueTask.DynamicContext();
                            if (mi.ReturnType.IsGenericType && valueTask.GetResult(mi.ReturnType.GetGenericArguments()[0]) is int exitCode)
                            {
                                if (Trace) WriteTrace($"API method returned exit code {exitCode}");
                                return exitCode;
                            }
                        }
                    }
                    if (Trace) WriteTrace("API method returned no exit code (using zero)");
                    return 0;
                }
                catch (Exception ex)
                {
                    WriteError($"API method failed with an exception: {ex}");
                    return await DisplayHelpAsync(new()
                    {
                        ExportedApis = exportedApis,
                        API = api,
                        Method = mi,
                        Parameters = [.. param],
                        Arguments = ca,
                        Exception = ex
                    }).DynamicContext();
                }
                finally
                {
                    CurrentContext = null;
                    if (api is IAsyncDisposable asyncDisposable)
                    {
                        await asyncDisposable.DisposeAsync().DynamicContext();
                    }
                    else if (api is IDisposable disposable)
                    {
                        disposable.Dispose();
                    }
                }
            }
            finally
            {
                if (removeExportedApis) ExportedApis = null;
            }
        }

        /// <summary>
        /// Display help
        /// </summary>
        /// <param name="context">CLI API context</param>
        /// <param name="useApi">Use the APIs error handling / help provider?</param>
        /// <returns>Exit code</returns>
        public static async Task<int> DisplayHelpAsync(
            CliApiContext context,
            bool useApi = true
            )
        {
            if (Trace) WriteTrace("Display context API help");
            if (context.Parameters is null || context.Exception is null || context.ForceDisplayHelp) DisplayHelpHeader();
            if (useApi)
            {
                // Try the API type error handler / help provider
                if (context.Arguments is not null)
                    if (context.Exception is not null && context.API is ICliApiErrorHandler errorHandler)
                    {
                        if (Trace) WriteTrace("Handle the API error using the executing API as error handler");
                        return await errorHandler.HandleApiErrorAsync(context).DynamicContext();
                    }
                    else if (context.API is ICliApiHelpProvider helpProvider)
                    {
                        if (Trace) WriteTrace("Display context help using the executing API as help provider");
                        return helpProvider.DisplayHelp(context);
                    }
                // Try the CliHelpApi
                if (context.API is not null && context.Arguments is not null && context.ExportedApis.FirstOrDefault(a => typeof(CliHelpApi).IsAssignableFrom(a)) is Type helpType)
                {
                    if (Trace) WriteTrace($"Using {helpType} as API context help provider");
                    ICliApiHelpProvider helpApi = (helpType.ConstructAuto() as ICliApiHelpProvider)
                        ?? throw new InvalidProgramException($"Failed to instance CLI help API {helpType}");
                    return helpApi.DisplayHelp(context);
                }
            }
            // Use the ICliApiHelper
            if (context.API is not null && context.Arguments is not null)
                if (context.Exception is not null && Helper is ICliApiErrorHandler errorHandler)
                {
                    if (Trace) WriteTrace("Handle the API error using the executing API as fallback error handler");
                    return await errorHandler.HandleApiErrorAsync(context).DynamicContext();
                }
                else if (Helper is ICliApiHelpProvider helpProvider)
                {
                    if (Trace) WriteTrace("Display context help using the executing API as fallback help provider");
                    return helpProvider.DisplayHelp(context);
                }
            if (Trace) WriteTrace($"Using the helper {Helper.GetType()} as API context help");
            return Helper.DisplayHelp(context);
        }

        /// <summary>
        /// Display the gerneral header message
        /// </summary>
        public static void DisplayGeneralHeader()
        {
            if (GeneralHeader is null || GeneralHeaderDisplayed || HelpHeaderDisplayed) return;
            GeneralHeaderDisplayed = true;
            AnsiConsole.MarkupLine(_(GeneralHeader));
            Console.WriteLine();
        }

        /// <summary>
        /// Display the help header message
        /// </summary>
        public static void DisplayHelpHeader()
        {
            if(HelpHeader is null)
            {
                DisplayGeneralHeader();
                return;
            }
            if (GeneralHeaderDisplayed || HelpHeaderDisplayed) return;
            HelpHeaderDisplayed = true;
            AnsiConsole.MarkupLine(_(HelpHeader));
            Console.WriteLine();
        }

        /// <summary>
        /// Delegate for a custom argument type parser (needs to throw on error)
        /// </summary>
        /// <param name="name">Argument name</param>
        /// <param name="type">Type</param>
        /// <param name="arg">Argument value</param>
        /// <param name="attr">Attribute</param>
        /// <returns>Parsed object</returns>
        public delegate object ParseType_Delegate(string name, Type type, string arg, CliApiAttribute attr);
    }
}
