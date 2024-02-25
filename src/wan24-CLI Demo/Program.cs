using wan24.CLI;
using wan24.CLI.Demo;
using wan24.Core;

await Bootstrap.Async().DynamicContext();
CliConfig.Apply(new(args));
Translation.Current = Translation.Dummy;
CliApi.HelpHeader = "[white]wan24-CLI Demo API help header[/]";
CliApi.CustomArgumentParsers[typeof(float)] = (name, type, arg, attr) => float.Parse(arg);
return await CliApi.RunAsync(args, default, typeof(CliHelpApi), typeof(DemoApi), typeof(AsyncDemoApi)).DynamicContext();
