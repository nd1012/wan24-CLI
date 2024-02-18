using Microsoft.Extensions.Logging;
using wan24.CLI;
using wan24.CLI.Demo;
using wan24.Core;

string fn = Path.GetFullPath("./demo.log");
if (File.Exists(fn)) File.Delete(fn);
await Bootstrap.Async().DynamicContext();
Logging.Logger = await FileLogger.CreateAsync(fn, LogLevel.Debug).DynamicContext();
return await CliApi.RunAsync(args, default, typeof(CliHelpApi), typeof(DemoApi)).DynamicContext();
