using wan24.Core;

namespace wan24.CLI
{
    /// <summary>
    /// CLI process helper
    /// </summary>
    public static class CliProcessHelper
    {
        /// <summary>
        /// Redirect STDOUT/-ERR from a running process to our STDOUT/-ERR
        /// </summary>
        /// <param name="proc">Process</param>
        /// <param name="stdOut">STDOUT</param>
        /// <param name="stdErr">STDERR</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public static async Task RedirectOutputAsync(ProcessStream proc, bool stdOut = true, bool stdErr = true, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if ((!stdOut || proc.StdOut is null) && (!stdErr || proc.StdErr is null))
            {
                if (proc.Process is not null) await proc.Process.WaitForExitAsync(cancellationToken).DynamicContext();
                return;
            }
            using CancellationTokenSource cts = new();
            using Cancellations cancellation = cancellationToken == default ? new(cts.Token) : new(cancellationToken, cts.Token);
            try
            {
                Task? stdOutRedirect = stdOut && proc.StdOut is not null ? proc.CopyToAsync(Console.OpenStandardOutput(), cancellation) : null,
                    stdErrRedirect = stdErr && proc.StdErr is not null ? proc.StdErr.BaseStream.CopyToAsync(Console.OpenStandardError(), cancellation) : null;
                if (proc.Process is not null) await proc.Process.WaitForExitAsync(cancellation).DynamicContext();
                if (stdOutRedirect is not null) await stdOutRedirect.WaitAsync(cancellation).DynamicContext();
                if (stdErrRedirect is not null) await stdErrRedirect.WaitAsync(cancellation).DynamicContext();
            }
            finally
            {
                if (!cts.IsCancellationRequested) cts.Cancel();
            }
        }
    }
}
