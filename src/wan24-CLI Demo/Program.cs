using Microsoft.Extensions.Logging;
using wan24.CLI;
using wan24.CLI.Demo;
using wan24.Core;

#if DEBUG
string fn = Path.GetFullPath("./demo.log");
if (File.Exists(fn)) File.Delete(fn);
#endif
await Bootstrap.Async().DynamicContext();
#if DEBUG
Logging.Logger = await FileLogger.CreateAsync(fn, LogLevel.Trace).DynamicContext();
#endif
Translation.Current = Translation.Dummy;
CliApi.HelpHeader = "[white]wan24-CLI Demo API help header[/]";
return await CliApi.RunAsync(args, default, typeof(CliHelpApi), typeof(DemoApi)).DynamicContext();
