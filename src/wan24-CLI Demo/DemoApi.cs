using Spectre.Console;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
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
            string message,
            int exitCode = 123
            )
        {
            AnsiConsole.MarkupLine(message);
            return exitCode;
        }

        [CliApi("echo2")]
        [DisplayText("Echo a message")]
        [Description("The output to STDOUT is the given message (Spectre.Console markup is supported), exit code will be v")]
        [StdOut("Message")]
        [ExitCode(213, "Default exit code")]
        public static int Echo2([CliApi] Echo2Arguments args, int exitCode = 213)
        {
            AnsiConsole.MarkupLine(args.Message);
            return exitCode;
        }

        [CliApi("echo3")]
        [DisplayText("Echo a message")]
        [Description("The output to STDOUT is the given message from STDIN (Spectre.Console markup is supported)")]
        [StdIn("Message", required: true)]
        [StdOut("Message")]
        public static void Echo3()
        {
            using StreamReader stdIn = new(Console.OpenStandardInput());
            string message = stdIn.ReadToEnd();
            try
            {
                AnsiConsole.MarkupLine(message);
            }
            catch(Exception ex)
            {
                Logging.WriteError($"{ex.Message}: {message}");
                throw;
            }
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
        public static int Sum2(
            [CliApi(ParseJson = true, Example = "1 2 3 ...")]
            [DisplayText("Numbers to summarize")]
            [Description("Define 1..n integer values to summarize")]
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

        [CliApi("custom")]
        [DisplayText("Custom type")]
        [Description("Demonstrate the usage of a custom argument type parser")]
        public static void CustomType(
            [CliApi(Example = "float")]
            [DisplayText("Number")]
            [Description("Float number to output to the console")]
            float number
            )
            => Console.WriteLine(number.ToString());

        [CliApi("dynamic")]
        [DisplayText("Dynamic arguments")]
        [Description("Demonstrate the usage of ConsoleIoHelper")]
        [StdIn("Input")]
        [StdOut("Output", required: true)]
        public static void DynamicArguments(
            [CliApi]
            [DisplayText("Input")]
            [Description("Input string")]
            string? input = null,
            [CliApi(ParseJson = true)]
            [DisplayText("Input format")]
            [Description("Input string format")]
            ConsoleIoHelper.Format inputFormat = ConsoleIoHelper.Format.String,
            [CliApi(ParseJson = true)]
            [DisplayText("Output format")]
            [Description("Output string format")]
            ConsoleIoHelper.Format outputFormat = ConsoleIoHelper.Format.String
            )
        {
            using Stream inputStream = ConsoleIoHelper.GetInput(input, format: inputFormat) ?? throw new ArgumentException("Input required", nameof(input));
            using MemoryPoolStream ms = new();
            inputStream.CopyTo(ms);
            ms.Position = 0;
            ConsoleIoHelper.SendOutput(ms, format: outputFormat);
        }

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
