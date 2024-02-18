using Spectre.Console;
using System.Collections.Frozen;
using System.Reflection;
using wan24.Core;
using static wan24.Core.Logging;

namespace wan24.CLI
{
    /// <summary>
    /// CLI API helper
    /// </summary>
    public static partial class CliApi
    {
        /// <summary>
        /// Helper (may implement <see cref="ICliApiHelpProvider"/> and/or <see cref="ICliApiErrorHandler"/>, too)
        /// </summary>
        public static ICliApiHelper Helper { get; set; } = CliApiHelper.Default;

        /// <summary>
        /// Use <c>InvokeAuth</c> for invoking API methods?
        /// </summary>
        public static bool UseInvokeAuto { get; set; } = true;

        /// <summary>
        /// Current CLI API context
        /// </summary>
        public static CliApiContext? CurrentContext { get; internal set; }

        /// <summary>
        /// Exported APIs
        /// </summary>
        public static FrozenDictionary<string, CliApiInfo>? ExportedApis { get; internal set; }

        /// <summary>
        /// Run the CLI API
        /// </summary>
        /// <param name="args">Argument list</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <param name="exportedApis">Exported APIs</param>
        /// <returns>Exit code</returns>
        public static async Task<int> RunAsync(string[] args, CancellationToken cancellationToken = default, params Type[] exportedApis)
        {
            await Task.Yield();
            cancellationToken.ThrowIfCancellationRequested();
            // Parse CLI arguments
            CliArguments ca;
            try
            {
                ca = (CliArguments)args;
            }
            catch (Exception ex)
            {
                Logging.WriteError($"Parsing CLI arguments failed: {ex}");
                return await DisplayHelpAsync(new()
                {
                    ExportedApis = exportedApis,
                    Exception = ex
                }).DynamicContext();
            }
            return await RunAsync(args, ca, cancellationToken, exportedApis).DynamicContext();
        }

        /// <summary>
        /// Run multiple CLI API calls (chunk arguments for API calls using a dash <c>-</c>; any parsing error or exit code <c>!=0</c> will cancel the execution loop; a call without 
        /// any argument isn't supported; calls will be processed sequential)
        /// </summary>
        /// <param name="args">Argument list</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <param name="exportedApis">Exported APIs</param>
        /// <returns>Exit code</returns>
        public static async Task<int> RunMultiAsync(string[] args, CancellationToken cancellationToken = default, params Type[] exportedApis)
        {
            await Task.Yield();
            cancellationToken.ThrowIfCancellationRequested();
            // Find exported APIs
            if (exportedApis.Length == 0)
            {
                if (Debug) Logging.WriteDebug("Find exported APIs");
                exportedApis = FindExportedApis();
                if (exportedApis.Length == 0 || exportedApis.Length == 1 && typeof(CliHelpApi).IsAssignableFrom(exportedApis[0]))
                    throw new InvalidProgramException("No CLI APIs exported");
            }
            ExportedApis = CliApiInfo.Create(exportedApis);
            // Display help
            if (args.Length == 0)
            {
                if (Debug) Logging.WriteDebug("No arguments given - display help");
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
                    if (Debug) Logging.WriteDebug("Found API call delimiter");
                    if (startArg == endArg)
                    {
                        Logging.WriteDebug("No arguments found - can't execute any API call");
                        continue;
                    }
                    exitCode = await RunAsync(args.AsSpan(startArg, endArg - startArg).ToArray(), cancellationToken, exportedApis).DynamicContext();
                    if (Debug) Logging.WriteDebug($"API call returned with exit code {exitCode}");
                    if (exitCode != 0)
                    {
                        AnsiConsole.MarkupLine($"[yellow on black]Break in arguments chunk {i + 1} with exit code {exitCode}[/]");
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

        /// <summary>
        /// Run the CLI API
        /// </summary>
        /// <param name="args">Argument list</param>
        /// <param name="ca">CLI arguments</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <param name="exportedApis">Exported APIs</param>
        /// <returns>Exit code</returns>
        public static async Task<int> RunAsync(string[] args, CliArguments ca, CancellationToken cancellationToken = default, params Type[] exportedApis)
        {
            await Task.Yield();
            cancellationToken.ThrowIfCancellationRequested();
            // Find exported APIs
            if (exportedApis.Length == 0)
            {
                if (Debug) Logging.WriteDebug("Find exported APIs");
                exportedApis = FindExportedApis();
                if (exportedApis.Length == 0 || exportedApis.Length == 1 && typeof(CliHelpApi).IsAssignableFrom(exportedApis[0]))
                    throw new InvalidProgramException("No CLI APIs exported");
            }
            ExportedApis = CliApiInfo.Create(exportedApis);
            // Find the CLI API to use
            int keyLessOffset = 0,
                keyLessArgOffset = 0;
            object api;
            if (exportedApis.Length == 1)
            {
                // Use the first and only API
                if (Debug) Logging.WriteDebug("Only one API is exported");
                api = exportedApis[0].ConstructAuto() ?? throw new InvalidProgramException($"Failed to instance CLI API {exportedApis[0]}");
            }
            else if (ca.KeyLessArguments.Count == 0)
            {
                // Use the default API
                Type type = FindDefaultApi(exportedApis) ?? exportedApis[0];
                if (Debug) Logging.WriteDebug($"Using default API {type}");
                api = type.ConstructAuto() ?? throw new InvalidProgramException($"Failed to instance CLI API {type}");
                if (api is null)
                {
                    if (Debug) Logging.WriteDebug($"Failed to instance default API {type}");
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
                    if (Debug) Logging.WriteDebug("API name not given");
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
                        if (Debug) Logging.WriteDebug($"Using named API {type} by attribute");
                    }
                    else if (!type.Name.Equals(apiName, StringComparison.CurrentCultureIgnoreCase))
                    {
                        continue;
                    }
                    else if(Debug)
                    {
                        Logging.WriteDebug($"Using named API {type} by class name");
                    }
                    api = type.ConstructAuto() ?? throw new InvalidProgramException($"Failed to instance CLI API {type}");
                    break;
                }
                if (api is null)
                {
                    if (Debug) Logging.WriteDebug($"Named API \"{apiName}\" not found");
                    return await DisplayHelpAsync(new()
                    {
                        ExportedApis = exportedApis,
                        Arguments = ca
                    }).DynamicContext();
                }
            }
            // Validate the required arguments
            try
            {
                MapArguments(api, ca, keyLessOffset, ref keyLessArgOffset);
            }
            catch (Exception ex)
            {
                Logging.WriteError($"Argument mapping failed with an exception: {ex}");
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
                if (Debug) Logging.WriteDebug("Only one API method is exported");
                mi = methods[0];
            }
            else if (ca.KeyLessArguments.Count == keyLessOffset || ca.KeyLessArguments[keyLessOffset] != args[keyLessOffset])
            {
                // Use the default method
                mi = FindDefaultApiMethod(methods)!;
                if (mi is null)
                {
                    if (Debug) Logging.WriteDebug("No default API method found");
                    return await DisplayHelpAsync(new()
                    {
                        ExportedApis = exportedApis,
                        API = api,
                        Arguments = ca
                    }).DynamicContext();
                }
                if (Debug) Logging.WriteDebug($"Using default API method {mi.Name}");
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
                    if (Debug) Logging.WriteDebug($"Named method \"{methodName}\" not found");
                    return await DisplayHelpAsync(new()
                    {
                        ExportedApis = exportedApis,
                        API = api,
                        Arguments = ca
                    }).DynamicContext();
                }
                if (Debug) Logging.WriteDebug($"Using named method {mi.Name}");
            }
            // Collect API method invokation parameters
            List<object?> param = [];
            if (UseInvokeAuto) param.Add(cancellationToken);
            try
            {
                object? value;
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
                        if (Debug) Logging.WriteDebug($"Mapping arguments to hosting object {value.GetType()}");
                        MapArguments(value, ca, keyLessOffset, ref keyLessArgOffset);
                        param.Add(value);
                    }
                    else
                    {
                        // Parsed argument
                        if (Debug) Logging.WriteDebug($"Using parsed argument {pi.Name} ({pi.ParameterType})");
                        param.Add(ParseArgument(pi.Name!, pi.ParameterType, ca, attr, keyLessOffset, ref keyLessArgOffset).Value);
                    }
            }
            catch (Exception ex)
            {
                Logging.WriteError($"Invokation parameer preparation failed with an exception: {ex}");
                return await DisplayHelpAsync(new()
                {
                    ExportedApis = exportedApis,
                    API = api,
                    Method = mi,
                    Arguments = ca,
                    Exception = ex
                }).DynamicContext();
            }
            // Invoke the API method and handle the result
            try
            {
                CurrentContext = new()
                {
                    ExportedApis = exportedApis,
                    API = api,
                    Method = mi,
                    Parameters = [.. param],
                    Arguments = ca
                };
                if (UseInvokeAuto)
                {
                    int res = (typeof(Task).IsAssignableFrom(mi.ReturnType)
                        ? await mi.InvokeAutoAsync(mi.IsStatic ? null : api, [.. param]).DynamicContext()
                        : mi.InvokeAuto(mi.IsStatic ? null : api, [.. param])) is int exitCode ? exitCode : 0;
                    if (Debug) Logging.WriteDebug($"API method returned exit code {res}");
                    return res;
                }
                object? ret = mi.Invoke(mi.IsStatic ? null : api, [.. param]);
                if (ret is not null)
                {
                    if (ret is int res)
                    {
                        if (Debug) Logging.WriteDebug($"API method returned exit code {res}");
                        return res;
                    }
                    if (ret is Task task)
                    {
                        await task.DynamicContext();
                        if (mi.ReturnType.IsGenericType && task.GetResult(mi.ReturnType.GetGenericArguments()[0]) is int exitCode)
                        {
                            if (Debug) Logging.WriteDebug($"API method returned exit code {exitCode}");
                            return exitCode;
                        }
                    }
                    else if (ret is ValueTask valueTask)
                    {
                        await valueTask.DynamicContext();
                        if (mi.ReturnType.IsGenericType && valueTask.GetResult(mi.ReturnType.GetGenericArguments()[0]) is int exitCode)
                        {
                            if (Debug) Logging.WriteDebug($"API method returned exit code {exitCode}");
                            return exitCode;
                        }
                    }
                }
                if (Debug) Logging.WriteDebug("API method returned no exit code (using zero)");
                return 0;
            }
            catch (Exception ex)
            {
                Logging.WriteError($"API method failed with an exception: {ex}");
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
            if (Debug) Logging.WriteDebug("Display context API help");
            if (useApi)
            {
                // Try the API type error handler / help provider
                if (context.Arguments is not null)
                    if (context.Exception is not null && context.API is ICliApiErrorHandler errorHandler)
                    {
                        if (Debug) Logging.WriteDebug("Handle the API error using the executing API as error handler");
                        return await errorHandler.HandleApiErrorAsync(context).DynamicContext();
                    }
                    else if (context.API is ICliApiHelpProvider helpProvider)
                    {
                        if (Debug) Logging.WriteDebug("Display context help using the executing API as help provider");
                        return helpProvider.DisplayHelp(context);
                    }
                // Try the CliHelpApi
                if (context.API is not null && context.Arguments is not null && context.ExportedApis.FirstOrDefault(a => typeof(CliHelpApi).IsAssignableFrom(a)) is Type helpType)
                {
                    if (Debug) Logging.WriteDebug($"Using {helpType} as API context help provider");
                    ICliApiHelpProvider helpApi = (helpType.ConstructAuto() as ICliApiHelpProvider)
                        ?? throw new InvalidProgramException($"Failed to instance CLI help API {helpType}");
                    return helpApi.DisplayHelp(context);
                }
            }
            // Use the ICliApiHelper
            if (context.API is not null && context.Arguments is not null)
                if (context.Exception is not null && Helper is ICliApiErrorHandler errorHandler)
                {
                    if (Debug) Logging.WriteDebug("Handle the API error using the executing API as fallback error handler");
                    return await errorHandler.HandleApiErrorAsync(context).DynamicContext();
                }
                else if (Helper is ICliApiHelpProvider helpProvider)
                {
                    if (Debug) Logging.WriteDebug("Display context help using the executing API as fallback help provider");
                    return helpProvider.DisplayHelp(context);
                }
            if (Debug) Logging.WriteDebug($"Using the helper {Helper.GetType()} as API context help");
            return Helper.DisplayHelp(context);
        }
    }
}
