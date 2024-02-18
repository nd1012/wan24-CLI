using Spectre.Console;
using System.ComponentModel;
using wan24.Core;

namespace wan24.CLI.Demo
{
    [CliApi("demo", IsDefault = true)]
    [DisplayText("Demo API")]
    [Description("This demonstration API is used for the wan24-CLI tests")]
    public sealed class DemoApi
    {
        public DemoApi() { }

        [CliApi]
        [DisplayText("Message")]
        [Description("The message to display by the Echo method")]
        public string? Message { get; set; }

        [CliApi("echo")]
        [DisplayText("Echo")]
        [Description("The output to STDOUT is the given message (Spectre.Console ANSI markup is supported), exit code will be 123")]
        [ExitCode(123, "Default exit code")]
        public int Echo()
        {
            if (Message is null) throw new InvalidOperationException("Missing message");
            AnsiConsole.MarkupLine(Message);
            return 123;
        }

        [CliApi]
        [DisplayText("Exit with code 123")]
        [Description("This API method will let the CLI app exit with code 123")]
        [ExitCode(123, "Default exit code")]
        public static int ExitCode() => 123;

        [CliApi]
        [DisplayText("Throw an exception")]
        [Description("This API method will throw an exception")]
        public static void Error() => throw new InvalidProgramException("Error API method called");
    }
}
