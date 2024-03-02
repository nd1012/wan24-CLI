using Microsoft.Extensions.Logging;
using wan24.Core;

namespace wan24_CLI_Tests
{
    [TestClass]
    public class A_Initialization
    {
        public static ILoggerFactory LoggerFactory { get; private set; } = null!;

        [AssemblyInitialize]
        public static void Init(TestContext tc)
        {
            if (File.Exists("tests.log")) File.Delete("tests.log");
            if (File.Exists("demo.log")) File.Delete("demo.log");
            Logging.Logger = new ConsoleLogger(LogLevel.Trace, next: FileLogger.CreateAsync("tests.log", LogLevel.Trace).Result);
            Logging.WriteInfo("wan24-CLI Tests initialized");
            Bootstrap.Async().Wait();
        }
    }
}
