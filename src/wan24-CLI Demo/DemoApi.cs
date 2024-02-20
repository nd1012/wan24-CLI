using Spectre.Console;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using wan24.Core;

namespace wan24.CLI.Demo
{
    [CliApi("demo")]
    [DisplayText("Demo API")]
    [Description("This demonstration API is used for the wan24-CLI tests")]
    public sealed class DemoApi
    {
        public DemoApi() { }

        [CliApi("echo")]
        [DisplayText("Echo a message")]
        [Description("The output to STDOUT is the given message (Spectre.Console markup is supported), exit code will be 123")]
        [StdOut("Message")]
        [ExitCode(123, "Default exit code")]
        public static int Echo(
            [CliApi]
            [DisplayText("Message")]
            [Description("The message to display (Spectre.Console markup is supported)")]
            string message
            )
        {
            AnsiConsole.MarkupLine(message);
            return 123;
        }

        [CliApi("echo2")]
        [DisplayText("Echo a message")]
        [Description("The output to STDOUT is the given message (Spectre.Console markup is supported), exit code will be 456")]
        [StdOut("Message")]
        [ExitCode(456, "Default exit code")]
        public static int Echo2([CliApi] Echo2Arguments args)
        {
            AnsiConsole.MarkupLine(args.Message);
            return 456;
        }

        [CliApi("sum")]
        [DisplayText("Summarize integers")]
        [Description("The given integers will be summarized")]
        [StdOut("Result")]
        public static void Sum(
            [CliApi(0)]
            [DisplayText("Numbers to summarize")]
            [Description("Define 1..n integer values to summarize")]
            string[] integers
            )
            => Console.WriteLine(integers.Select(int.Parse).Sum().ToString());

        [CliApi("sum2")]
        [DisplayText("Summarize integers")]
        [Description("The given integers will be summarized, the result will be the exit code")]
        [StdOut("Result")]
        public static int Sum2(
            [CliApi(ParseJson = true, Example = "\"[ 1, 2, 3, ... ]\"")]
            [DisplayText("Numbers to summarize")]
            [Description("Define 1..n integer values as JSON array to summarize")]
            int[] integers
            )
            => integers.Sum();

        [CliApi("exit")]
        [DisplayText("Exit with code -123")]
        [Description("This API method will let the CLI app exit with code -123")]
        [ExitCode(123, "Default exit code")]
        public static int ExitCode() => -123;

        [CliApi("error")]
        [DisplayText("Throw an exception")]
        [Description("This API method will throw an exception")]
        public static void Error() => throw new InvalidProgramException("Error API method called");

        public sealed record class Echo2Arguments : ICliArguments
        {
            [CliApi]
            [DisplayText("Message")]
            [Description("The message to display (Spectre.Console markup is supported)")]
            [Required]
            public string Message { get; set; } = null!;
        }
    }
}
