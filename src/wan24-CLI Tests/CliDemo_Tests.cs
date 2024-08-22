using wan24.Core;
using wan24.Tests;

namespace wan24_CLI_Tests
{
    [TestClass]
    public class CliDemo_Tests : TestBase
    {
        [TestMethod, Timeout(10000)]
        public void General_Tests()
        {
            string configuration =
#if DEBUG
                "Debug"
#elif TRUNK
                "Trunk"
#else
                "Release"
#endif
                ,
                cmd = $"dotnet",
                app = Path.GetFullPath($"../../../../wan24-CLI Demo/bin/{configuration}/net8.0/wan24CliDemo.dll"),
                cl,
                stdOut,
                stdErr;
            Console.WriteLine($"CMD: {cmd} \"{app}\"");
            string[] defaultArgs = ["--wan24.Core.Settings.LogLevel", "Debug", "--wan24.Core.CliConfig.LoggerType", "wan24.CLI.VividConsoleLogger", "--wan24.Core.CliConfig.LogFile", "demo.log"];

            // Echo
            using (ProcessStream proc = ProcessStream.Create(cmd, args: [app, "demo", "echo", "--message", "test", .. defaultArgs]))
            {
                proc.Process!.WaitForExit();
                stdOut = proc.StdOut!.ReadToEnd();
                stdErr = proc.StdErr!.ReadToEnd();
                cl = $"{proc.Process.StartInfo.FileName} {string.Join(' ', proc.Process.StartInfo.ArgumentList)}";
                Console.WriteLine($"CMD: {cl}");
                Console.WriteLine($"STDOUT: {stdOut}");
                Console.WriteLine($"STDERR: {stdErr}");
                Assert.AreEqual(123, proc.Process.ExitCode);
                Assert.AreEqual("test", stdOut.Trim());
            }

            // Echo 2
            using (ProcessStream proc = ProcessStream.Create(cmd, args: [app, "demo", "echo2", "--message", "test", .. defaultArgs]))
            {
                proc.Process!.WaitForExit();
                stdOut = proc.StdOut!.ReadToEnd();
                stdErr = proc.StdErr!.ReadToEnd();
                cl = $"{proc.Process.StartInfo.FileName} {string.Join(' ', proc.Process.StartInfo.ArgumentList)}";
                Console.WriteLine($"CMD: {cl}");
                Console.WriteLine($"STDOUT: {stdOut}");
                Console.WriteLine($"STDERR: {stdErr}");
                Assert.AreEqual(213, proc.Process.ExitCode);
                Assert.AreEqual("test", stdOut.Trim());
            }

            // Echo 3
            using (ProcessStream proc = ProcessStream.Create(cmd, useStdIn: true, args: [app, "demo", "echo3", .. defaultArgs]))
            {
                proc.StdIn!.Write("test");
                proc.StdIn.Close();
                proc.Process!.WaitForExit();
                stdOut = proc.StdOut!.ReadToEnd();
                stdErr = proc.StdErr!.ReadToEnd();
                cl = $"{proc.Process.StartInfo.FileName} {string.Join(' ', proc.Process.StartInfo.ArgumentList)}";
                Console.WriteLine($"CMD: {cl}");
                Console.WriteLine($"STDOUT: {stdOut}");
                Console.WriteLine($"STDERR: {stdErr}");
                Assert.AreEqual(0, proc.Process.ExitCode);
                Assert.AreEqual("test", stdOut.Trim());
            }

            // Sum
            using (ProcessStream proc = ProcessStream.Create(cmd, args: [app, "demo", "sum", "1", "2", "3", .. defaultArgs]))
            {
                proc.Process!.WaitForExit();
                stdOut = proc.StdOut!.ReadToEnd();
                stdErr = proc.StdErr!.ReadToEnd();
                cl = $"{proc.Process.StartInfo.FileName} {string.Join(' ', proc.Process.StartInfo.ArgumentList)}";
                Console.WriteLine($"CMD: {cl}");
                Console.WriteLine($"STDOUT: {stdOut}");
                Console.WriteLine($"STDERR: {stdErr}");
                Assert.AreEqual(0, proc.Process.ExitCode);
                Assert.AreEqual("6", stdOut.Trim());
            }

            // Sum2
            using (ProcessStream proc = ProcessStream.Create(cmd, args: [app, "demo", "sum2", "--integers", "1", "2", "3", .. defaultArgs]))
            {
                proc.Process!.WaitForExit();
                stdOut = proc.StdOut!.ReadToEnd();
                stdErr = proc.StdErr!.ReadToEnd();
                cl = $"{proc.Process.StartInfo.FileName} {string.Join(' ', proc.Process.StartInfo.ArgumentList)}";
                Console.WriteLine($"CMD: {cl}");
                Console.WriteLine($"STDOUT: {stdOut}");
                Console.WriteLine($"STDERR: {stdErr}");
                Assert.AreEqual(6, proc.Process.ExitCode);
            }

            // Error
            using (ProcessStream proc = ProcessStream.Create(cmd, args: [app, "demo", "error", .. defaultArgs]))
            {
                proc.Process!.WaitForExit();
                stdOut = proc.StdOut!.ReadToEnd();
                stdErr = proc.StdErr!.ReadToEnd();
                cl = $"{proc.Process.StartInfo.FileName} {string.Join(' ', proc.Process.StartInfo.ArgumentList)}";
                Console.WriteLine($"CMD: {cl}");
                Console.WriteLine($"STDOUT: {stdOut}");
                Console.WriteLine($"STDERR: {stdErr}");
                Assert.AreEqual(1, proc.Process.ExitCode);
            }

            // Custom argument type
            using (ProcessStream proc = ProcessStream.Create(cmd, args: [app, "demo", "custom", "--number", 0.123f.ToString(), .. defaultArgs]))
            {
                proc.Process!.WaitForExit();
                stdOut = proc.StdOut!.ReadToEnd();
                stdErr = proc.StdErr!.ReadToEnd();
                cl = $"{proc.Process.StartInfo.FileName} {string.Join(' ', proc.Process.StartInfo.ArgumentList)}";
                Console.WriteLine($"CMD: {cl}");
                Console.WriteLine($"STDOUT: {stdOut}");
                Console.WriteLine($"STDERR: {stdErr}");
                Assert.AreEqual(0, proc.Process.ExitCode);
                Assert.AreEqual(0.123f, float.Parse(stdOut.Trim()));
            }

            // Dynamic arguments
            using (ProcessStream proc = ProcessStream.Create(cmd, args: [
                app, 
                "demo", 
                "dynamic", 
                "--input", 
                Convert.ToBase64String("test".GetBytes()), 
                "--inputFormat",
                "Base64",
                "--outputFormat",
                "Hex",
                .. defaultArgs
                ]))
            {
                proc.Process!.WaitForExit();
                stdOut = proc.StdOut!.ReadToEnd();
                stdErr = proc.StdErr!.ReadToEnd();
                cl = $"{proc.Process.StartInfo.FileName} {string.Join(' ', proc.Process.StartInfo.ArgumentList)}";
                Console.WriteLine($"CMD: {cl}");
                Console.WriteLine($"STDOUT: {stdOut}");
                Console.WriteLine($"STDERR: {stdErr}");
                Assert.AreEqual(0, proc.Process.ExitCode);
                Assert.AreEqual("test", Convert.FromHexString(stdOut).ToUtf8String());
            }
        }

        [TestMethod, Timeout(10000)]
        public async Task GeneralAsync_Tests()
        {
            string configuration =
#if DEBUG
                "Debug"
#elif TRUNK
                "Trunk"
#else
                "Release"
#endif
                ,
                cmd = $"dotnet",
                app = Path.GetFullPath($"../../../../wan24-CLI Demo/bin/{configuration}/net8.0/wan24CliDemo.dll"),
                cl,
                stdOut,
                stdErr;
            Console.WriteLine($"CMD: {cmd} \"{app}\"");
            string[] defaultArgs = ["--wan24.Core.Settings.LogLevel", "Debug", "--wan24.Core.CliConfig.LoggerType", "wan24.CLI.VividConsoleLogger", "--wan24.Core.CliConfig.LogFile", "demo.log"];

            // Echo
            using (ProcessStream proc = ProcessStream.Create(cmd, args: [app, "asyncdemo", "echo", "--message", "test", .. defaultArgs]))
            {
                await proc.Process!.WaitForExitAsync().DynamicContext();
                stdOut = await proc.StdOut!.ReadToEndAsync().DynamicContext();
                stdErr = await proc.StdErr!.ReadToEndAsync().DynamicContext();
                cl = $"{proc.Process.StartInfo.FileName} {string.Join(' ', proc.Process.StartInfo.ArgumentList)}";
                Console.WriteLine($"CMD: {cl}");
                Console.WriteLine($"STDOUT: {stdOut}");
                Console.WriteLine($"STDERR: {stdErr}");
                Assert.AreEqual(123, proc.Process.ExitCode);
                Assert.AreEqual("test", stdOut.Trim());
            }

            // Echo 2
            using (ProcessStream proc = ProcessStream.Create(cmd, args: [app, "asyncdemo", "echo2", "--message", "test", .. defaultArgs]))
            {
                await proc.Process!.WaitForExitAsync().DynamicContext();
                stdOut = await proc.StdOut!.ReadToEndAsync().DynamicContext();
                stdErr = await proc.StdErr!.ReadToEndAsync().DynamicContext();
                cl = $"{proc.Process.StartInfo.FileName} {string.Join(' ', proc.Process.StartInfo.ArgumentList)}";
                Console.WriteLine($"CMD: {cl}");
                Console.WriteLine($"STDOUT: {stdOut}");
                Console.WriteLine($"STDERR: {stdErr}");
                Assert.AreEqual(213, proc.Process.ExitCode);
                Assert.AreEqual("test", stdOut.Trim());
            }

            // Echo 3
            using (ProcessStream proc = ProcessStream.Create(cmd, useStdIn: true, args: [app, "asyncdemo", "echo3", .. defaultArgs]))
            {
                await proc.StdIn!.WriteAsync("test").DynamicContext();
                proc.StdIn.Close();
                await proc.Process!.WaitForExitAsync().DynamicContext();
                stdOut = await proc.StdOut!.ReadToEndAsync().DynamicContext();
                stdErr = await proc.StdErr!.ReadToEndAsync().DynamicContext();
                cl = $"{proc.Process.StartInfo.FileName} {string.Join(' ', proc.Process.StartInfo.ArgumentList)}";
                Console.WriteLine($"CMD: {cl}");
                Console.WriteLine($"STDOUT: {stdOut}");
                Console.WriteLine($"STDERR: {stdErr}");
                Assert.AreEqual(0, proc.Process.ExitCode);
                Assert.AreEqual("test", stdOut.Trim());
            }

            // Sum
            using (ProcessStream proc = ProcessStream.Create(cmd, args: [app, "asyncdemo", "sum", "1", "2", "3", .. defaultArgs]))
            {
                await proc.Process!.WaitForExitAsync().DynamicContext();
                stdOut = await proc.StdOut!.ReadToEndAsync().DynamicContext();
                stdErr = await proc.StdErr!.ReadToEndAsync().DynamicContext();
                cl = $"{proc.Process.StartInfo.FileName} {string.Join(' ', proc.Process.StartInfo.ArgumentList)}";
                Console.WriteLine($"CMD: {cl}");
                Console.WriteLine($"STDOUT: {stdOut}");
                Console.WriteLine($"STDERR: {stdErr}");
                Assert.AreEqual(0, proc.Process.ExitCode);
                Assert.AreEqual("6", stdOut.Trim());
            }

            // Sum2
            using (ProcessStream proc = ProcessStream.Create(cmd, args: [app, "asyncdemo", "sum2", "--integers", "1", "2", "3", .. defaultArgs]))
            {
                await proc.Process!.WaitForExitAsync().DynamicContext();
                stdOut = await proc.StdOut!.ReadToEndAsync().DynamicContext();
                stdErr = await proc.StdErr!.ReadToEndAsync().DynamicContext();
                cl = $"{proc.Process.StartInfo.FileName} {string.Join(' ', proc.Process.StartInfo.ArgumentList)}";
                Console.WriteLine($"CMD: {cl}");
                Console.WriteLine($"STDOUT: {stdOut}");
                Console.WriteLine($"STDERR: {stdErr}");
                Assert.AreEqual(6, proc.Process.ExitCode);
            }

            // Error
            using (ProcessStream proc = ProcessStream.Create(cmd, args: [app, "asyncdemo", "error", .. defaultArgs]))
            {
                await proc.Process!.WaitForExitAsync().DynamicContext();
                stdOut = await proc.StdOut!.ReadToEndAsync().DynamicContext();
                stdErr = await proc.StdErr!.ReadToEndAsync().DynamicContext();
                cl = $"{proc.Process.StartInfo.FileName} {string.Join(' ', proc.Process.StartInfo.ArgumentList)}";
                Console.WriteLine($"CMD: {cl}");
                Console.WriteLine($"STDOUT: {stdOut}");
                Console.WriteLine($"STDERR: {stdErr}");
                Assert.AreEqual(1, proc.Process.ExitCode);
            }

            // Custom argument type
            using (ProcessStream proc = ProcessStream.Create(cmd, args: [app, "asyncdemo", "custom", "--number", 0.123f.ToString(), .. defaultArgs]))
            {
                await proc.Process!.WaitForExitAsync().DynamicContext();
                stdOut = await proc.StdOut!.ReadToEndAsync().DynamicContext();
                stdErr = await proc.StdErr!.ReadToEndAsync().DynamicContext();
                cl = $"{proc.Process.StartInfo.FileName} {string.Join(' ', proc.Process.StartInfo.ArgumentList)}";
                Console.WriteLine($"CMD: {cl}");
                Console.WriteLine($"STDOUT: {stdOut}");
                Console.WriteLine($"STDERR: {stdErr}");
                Assert.AreEqual(0, proc.Process.ExitCode);
                Assert.AreEqual(0.123f, float.Parse(stdOut.Trim()));
            }

            // Dynamic arguments
            using (ProcessStream proc = ProcessStream.Create(cmd, args: [
                app,
                "asyncdemo",
                "dynamic",
                "--input",
                Convert.ToBase64String("test".GetBytes()),
                "--inputFormat",
                "Base64",
                "--outputFormat",
                "Hex",
                .. defaultArgs
                ]))
            {
                await proc.Process!.WaitForExitAsync().DynamicContext();
                stdOut = await proc.StdOut!.ReadToEndAsync().DynamicContext();
                stdErr = await proc.StdErr!.ReadToEndAsync().DynamicContext();
                cl = $"{proc.Process.StartInfo.FileName} {string.Join(' ', proc.Process.StartInfo.ArgumentList)}";
                Console.WriteLine($"CMD: {cl}");
                Console.WriteLine($"STDOUT: {stdOut}");
                Console.WriteLine($"STDERR: {stdErr}");
                Assert.AreEqual(0, proc.Process.ExitCode);
                Assert.AreEqual("test", Convert.FromHexString(stdOut).ToUtf8String());
            }
        }
    }
}
