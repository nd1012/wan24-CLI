using wan24.Core;

namespace wan24.CLI
{
    /// <summary>
    /// CLI stream (STDIN/-OUT)
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="leaveOpen">Leave the STDIN/-OUT streams open when disposing?</param>
    public class CliStream(in bool leaveOpen = false) : BiDirectionalStream(Console.OpenStandardInput(), Console.OpenStandardOutput(), leaveOpen)
    {
    }
}
