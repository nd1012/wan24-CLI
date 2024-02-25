using Spectre.Console;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using wan24.Core;

namespace wan24.CLI.Demo
{
    [CliApi("asyncdemo")]
    [DisplayText("Asynchronous demo API")]
    [Description("This demonstration API is used for the wan24-CLI tests")]
    public sealed class AsyncDemoApi
    {
        public AsyncDemoApi() { }

        [CliApi("echo")]
        [DisplayText("Echo a message")]
        [Description("The output to STDOUT is the given message (Spectre.Console markup is supported), exit code will be 123")]
        [StdOut("Message")]
        [ExitCode(123, "Default exit code")]
        public static Task<int> EchoAsync(
            [CliApi]
            [DisplayText("Message")]
            [Description("The message to display (Spectre.Console markup is supported)")]
            string message,
            int exitCode = 123
            )
        {
            AnsiConsole.MarkupLine(message);
            return Task.FromResult(exitCode);
        }

        [CliApi("echo2")]
        [DisplayText("Echo a message")]
        [Description("The output to STDOUT is the given message (Spectre.Console markup is supported), exit code will be 456")]
        [StdOut("Message")]
        [ExitCode(456, "Default exit code")]
        public static Task<int> Echo2Async([CliApi] Echo2Arguments args, int exitCode = 456)
        {
            AnsiConsole.MarkupLine(args.Message);
            return Task.FromResult(exitCode);
        }

        [CliApi("echo3")]
        [DisplayText("Echo a message")]
        [Description("The output to STDOUT is the given message from STDIN (Spectre.Console markup is supported)")]
        [StdIn("Message", required: true)]
        [StdOut("Message")]
        public static Task Echo3Async()
        {
            using StreamReader stdIn = new(Console.OpenStandardInput());
            string message = stdIn.ReadToEnd();
            try
            {
                AnsiConsole.MarkupLine(message);
            }
            catch (Exception ex)
            {
                Logging.WriteError($"{ex.Message}: {message}");
                throw;
            }
            return Task.CompletedTask;
        }

        [CliApi("sum")]
        [DisplayText("Summarize integers")]
        [Description("The given integers will be summarized")]
        [StdOut("Result")]
        public static Task SumAsync(
            [CliApi(0)]
            [DisplayText("Numbers to summarize")]
            [Description("Define 1..n integer values to summarize")]
            string[] integers
            )
        {
            Console.WriteLine(integers.Select(int.Parse).Sum().ToString());
            return Task.CompletedTask;
        }

        [CliApi("sum2")]
        [DisplayText("Summarize integers")]
        [Description("The given integers will be summarized, the result will be the exit code")]
        public static Task<int> Sum2Async(
            [CliApi(ParseJson = true, Example = "1 2 3 ...")]
            [DisplayText("Numbers to summarize")]
            [Description("Define 1..n integer values to summarize")]
            int[] integers
            )
            => Task.FromResult(integers.Sum());

        [CliApi("exit")]
        [DisplayText("Exit with code -123")]
        [Description("This API method will let the CLI app exit with code -123")]
        [ExitCode(123, "Default exit code")]
        public static Task<int> ExitCodeAsync() => Task.FromResult(-123);

        [CliApi("error")]
        [DisplayText("Throw an exception")]
        [Description("This API method will throw an exception")]
        public static Task ErrorAsync() => throw new InvalidProgramException("Error API method called");

        [CliApi("custom")]
        [DisplayText("Custom type")]
        [Description("Demonstrate the usage of a custom argument type parser")]
        public static Task CustomTypeAsync(
            [CliApi(Example = "float")]
            [DisplayText("Number")]
            [Description("Float number to output to the console")]
            float number
            )
        {
            Console.WriteLine(number.ToString());
            return Task.CompletedTask;
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
