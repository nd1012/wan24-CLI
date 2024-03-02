using Spectre.Console;
using System.Text;
using wan24.Core;
using static wan24.Core.Logging;
using static wan24.Core.Logger;

namespace wan24.CLI
{
    /// <summary>
    /// Console I/O helper
    /// </summary>
    public static class ConsoleIoHelper
    {
        /// <summary>
        /// Get console input
        /// </summary>
        /// <param name="value">Given value (may be used as input filename, if <c>format</c> contains <see cref="Format.File"/>; may be used as environment variable name, if <c>format</c> contains 
        /// <see cref="Format.Environment"/>; may be converted depending on the given <c>format</c>)</param>
        /// <param name="useStdIn">Use STDIN (won't be decoded)?</param>
        /// <param name="envName">Environment variable name (will look into process, user and machine variables)</param>
        /// <param name="inputPrompt">User input prompt (set to try user input; Spectre.Console markup is supported)</param>
        /// <param name="isSecret">If the user input is a secret</param>
        /// <param name="secretMask">User secret input mask character (optional)</param>
        /// <param name="allowEmpty">Allow an empty user input?</param>
        /// <param name="format">Input format (user input or environment variable will be decoded accordingly)</param>
        /// <returns>Input stream (is <see langword="null"/>, if no input was found; deserialization must be done from the calling code!)</returns>
        public static Stream? GetInput(
            string? value = null,
            in bool useStdIn = true,
            string? envName = null,
            in string? inputPrompt = null,
            in bool isSecret = false,
            in char? secretMask = null,
            in bool allowEmpty = false,
            Format format = Format.String
            )
        {
            if (Trace) WriteTrace("Getting input");
            // Interpret the value, if given
            if (value is not null)
            {
                if (Trace) WriteTrace("Input value given");
                if (format == Format.File)
                {
                    if (Trace) WriteTrace("Loading file");
                    return FsHelper.CreateFileStream(Path.GetFullPath(value), FileMode.Open, FileAccess.Read, FileShare.Read);
                }
                if (format.ContainsAnyFlag(Format.Environment))
                {
                    if (Trace) WriteTrace("Switching to environment variable input");
                    envName = value;
                    value = null;
                }
            }
            format &= Format.ENCODING_FLAGS;
            if (format.GetContainedFlags([.. EnumInfo<Format>.Values]).Where(f => f != Format.Binary).Count() > 1)
                throw new ArgumentException("Only one format is allowed", nameof(format));
            // Try environment variables
            if (value is null && envName is not null)
            {
                if (Trace) WriteTrace("Trying environment variables");
                foreach (EnvironmentVariableTarget target in EnumInfo<EnvironmentVariableTarget>.Values.OrderBy(v => (int)v))
                {
                    value = Environment.GetEnvironmentVariable(envName, target);
                    if (value is not null)
                    {
                        if (Trace) WriteTrace($"Using environment variable \"{envName}\" (from {target})");
                        break;
                    }
                }
            }
            // Try user input
            if (value is null && inputPrompt is not null)
            {
                if (Trace) WriteTrace("Trying user input");
                value = AnsiConsole.Prompt(isSecret
                    ? allowEmpty && !useStdIn
                        ? new TextPrompt<string>(inputPrompt).Secret(secretMask).AllowEmpty()
                        : new TextPrompt<string>(inputPrompt).Secret(secretMask)
                    : allowEmpty && !useStdIn
                        ? new TextPrompt<string>(inputPrompt).AllowEmpty()
                        : new TextPrompt<string>(inputPrompt));
                if (useStdIn && value.Length == 0)
                {
                    if (Trace) WriteTrace("No user input");
                    value = null;
                }
            }
            // Apply decoding, if required and possible
            if (Trace)
                if (value is null)
                {
                    WriteTrace("No input value yet");
                    if (useStdIn) WriteTrace("Using STDIN, finally");
                }
                else
                {
                    WriteTrace($"Apply {format} decoding on input value");
                }
            return value is null
                ? useStdIn
                    ? Console.OpenStandardInput()
                    : null
                : format switch
                {
                    Format.Base64 => new MemoryStream(value.AsSpan().DecodeBase64()),
                    Format.Hex => new MemoryStream(value.GetBytesFromHex()),
                    Format.ByteEncoding => new MemoryStream(value.Decode()),
                    _ => new MemoryStream(value.GetBytes())
                };
        }

        /// <summary>
        /// Send output from a stream (NO Spectre.Console markup is supported!)
        /// </summary>
        /// <param name="stream">Data stream (will be encoded depending on the given <c>format</c>, if not <see cref="Format.Binary"/> or <see cref="Format.String"/>)</param>
        /// <param name="output">Output stream (will be disposed)</param>
        /// <param name="fileName">Output filename (if <see langword="null"/>, and <c>output</c> is <see langword="null"/> also, STDOUT will be used</param>
        /// <param name="format">Output format</param>
        /// <param name="exitCode">Exit code</param>
        /// <returns>Exit code</returns>
        public static int SendOutput(in Stream stream, Stream? output = null, in string? fileName = null, in Format format = Format.Binary, in int exitCode = 0)
        {
            if (Trace) WriteTrace("Sending output");
            output ??= fileName is null ? Console.OpenStandardOutput() : FsHelper.CreateFileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None, overwrite: true);
            try
            {
                switch (format & Format.ENCODING_FLAGS)
                {
                    case Format.Binary:
                    case Format.String:
                        if (Trace) WriteTrace("Output is unencoded");
                        stream.CopyTo(output);
                        break;
                    case Format.Base64:
                        {
                            if (Trace) WriteTrace("Encode output using base64");
                            using MemoryPoolStream ms = new()
                            {
                                CleanReturned = true
                            };
                            stream.CopyTo(ms);
                            output.Write(ms.ToArray().GetBase64Bytes().AsSpan());
                        }
                        break;
                    case Format.Hex:
                        {
                            if (Trace) WriteTrace("Encode output using hex");
                            using MemoryPoolStream ms = new()
                            {
                                CleanReturned = true
                            };
                            stream.CopyTo(ms);
                            output.Write(Convert.ToHexString(ms.ToArray().AsSpan()).GetBytes().AsSpan());
                        }
                        break;
                    case Format.ByteEncoding:
                        {
                            if (Trace) WriteTrace("Encode output using byte encoding");
                            using MemoryPoolStream ms = new()
                            {
                                CleanReturned = true
                            };
                            stream.CopyTo(ms);
                            output.Write(ms.ToArray().AsSpan().Encode().GetBytes().AsSpan());
                        }
                        break;
                    default:
                        throw new ArgumentException($"Invalid format \"{format}\"", nameof(format));
                }
                return exitCode;
            }
            finally
            {
                output.Dispose();
            }
        }

        /// <summary>
        /// Send output from a stream (NO Spectre.Console markup is supported!)
        /// </summary>
        /// <param name="stream">Data stream (will be encoded depending on the given <c>format</c>, if not <see cref="Format.Binary"/> or <see cref="Format.String"/>)</param>
        /// <param name="output">Output stream (will be disposed)</param>
        /// <param name="fileName">Output filename (if <see langword="null"/>, and <c>output</c> is <see langword="null"/> also, STDOUT will be used</param>
        /// <param name="format">Output format</param>
        /// <param name="exitCode">Exit code</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Exit code</returns>
        public static async Task<int> SendOutputAsync(
            Stream stream,
            Stream? output = null,
            string? fileName = null,
            Format format = Format.Binary,
            int exitCode = 0,
            CancellationToken cancellationToken = default
            )
        {
            if (Trace) WriteTrace("Sending output");
            output ??= fileName is null ? Console.OpenStandardOutput() : FsHelper.CreateFileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None, overwrite: true);
            try
            {
                switch (format & Format.ENCODING_FLAGS)
                {
                    case Format.Binary:
                    case Format.String:
                        if (Trace) WriteTrace("Output is unencoded");
                        await stream.CopyToAsync(output, cancellationToken).DynamicContext();
                        break;
                    case Format.Base64:
                        {
                            if (Trace) WriteTrace("Encode output using base64");
                            using MemoryPoolStream ms = new()
                            {
                                CleanReturned = true
                            };
                            await stream.CopyToAsync(ms, cancellationToken).DynamicContext();
                            await output.WriteAsync(ms.ToArray().GetBase64Bytes().AsMemory(), cancellationToken).DynamicContext();
                        }
                        break;
                    case Format.Hex:
                        {
                            if (Trace) WriteTrace("Encode output using hex");
                            using MemoryPoolStream ms = new()
                            {
                                CleanReturned = true
                            };
                            await stream.CopyToAsync(ms, cancellationToken).DynamicContext();
                            await output.WriteAsync(Convert.ToHexString(ms.ToArray().AsSpan()).GetBytes().AsMemory(), cancellationToken).DynamicContext();
                        }
                        break;
                    case Format.ByteEncoding:
                        {
                            if (Trace) WriteTrace("Encode output using byte encoding");
                            using MemoryPoolStream ms = new()
                            {
                                CleanReturned = true
                            };
                            await stream.CopyToAsync(ms, cancellationToken).DynamicContext();
                            await output.WriteAsync(ms.ToArray().AsSpan().Encode().GetBytes().AsMemory(), cancellationToken).DynamicContext();
                        }
                        break;
                    default:
                        throw new ArgumentException($"Invalid format \"{format}\"", nameof(format));
                }
                return exitCode;
            }
            finally
            {
                await output.DisposeAsync().DynamicContext();
            }
        }

        /// <summary>
        /// Get the object serializer from a format
        /// </summary>
        /// <param name="format">Format</param>
        /// <param name="defaultSerializer">Default serializer</param>
        /// <returns>Object serialzer</returns>
        public static ObjectSerializer.Serializer GetObjectSerializer(this Format format, in ObjectSerializer.Serializer defaultSerializer = ObjectSerializer.Serializer.Json)
        {
            if (format.ContainsAnyFlag(Format.Json)) return ObjectSerializer.Serializer.Json;
            else if (format.ContainsAnyFlag(Format.Xml)) return ObjectSerializer.Serializer.Xml;
            else return defaultSerializer;
        }

        /// <summary>
        /// Determine if the format contains serializer instructions
        /// </summary>
        /// <param name="format">Format</param>
        /// <returns>If serializer instructions are contained</returns>
        public static bool ContainsSerializer(this Format format) => (format & Format.SERIALIZER_FLAGS) != 0;

        /// <summary>
        /// Get the consle I/O helper serializer format enumeration value
        /// </summary>
        /// <param name="serializer">Serializer</param>
        /// <returns>Format flag</returns>
        public static Format GetSerializerFormat(this ObjectSerializer.Serializer serializer) => serializer switch
        {
            ObjectSerializer.Serializer.Json => Format.Json,
            ObjectSerializer.Serializer.Xml => Format.Xml,
            _ => throw new ArgumentException($"Invalid serializer \"{serializer}\"", nameof(serializer))
        };

        /// <summary>
        /// I/O exchange formats enumeration
        /// </summary>
        [Flags]
        public enum Format : int
        {
            /// <summary>
            /// Binary (raw, without any encoding/decoding)
            /// </summary>
            [DisplayText("Binary (raw, without any encoding/decoding)")]
            Binary = 0,
            /// <summary>
            /// String (UTF-8)
            /// </summary>
            [DisplayText("String (UTF-8)")]
            String = 1,
            /// <summary>
            /// base64
            /// </summary>
            [DisplayText("base64")]
            Base64 = 2,
            /// <summary>
            /// Hexadecimal
            /// </summary>
            [DisplayText("Hexadecimal")]
            Hex = 4,
            /// <summary>
            /// <see cref="Core.ByteEncoding"/> (UTF-8)
            /// </summary>
            [DisplayText("Byte encoded (UTF-8)")]
            ByteEncoding = 8,
            /// <summary>
            /// Input source is a file (not or UTF-8 encoded)
            /// </summary>
            [DisplayText("Input source is a file (not or UTF-8 encoded)")]
            File = 16,
            /// <summary>
            /// Input source is an environment variable
            /// </summary>
            [DisplayText("Input source is an environment variable")]
            Environment = 32,
            /// <summary>
            /// Higher level JSON data format (must be deserialized from supporting code)
            /// </summary>
            [DisplayText("JSON data format")]
            Json = 64,
            /// <summary>
            /// Higher level XML data format (must be deserialized from supporting code)
            /// </summary>
            [DisplayText("XML data format")]
            Xml = 128,
            /// <summary>
            /// Encoding flags
            /// </summary>
            [DisplayText("All encodings")]
            ENCODING_FLAGS = String | Base64 | Hex | ByteEncoding,
            /// <summary>
            /// Input source flags
            /// </summary>
            [DisplayText("All input sources")]
            INPUT_FLAGS = File | Environment,
            /// <summary>
            /// Serializer type flags
            /// </summary>
            [DisplayText("All serializers")]
            SERIALIZER_FLAGS = Json | Xml,
            /// <summary>
            /// All flags
            /// </summary>
            [DisplayText("All flags")]
            FLAGS = ENCODING_FLAGS | INPUT_FLAGS | SERIALIZER_FLAGS
        }
    }
}
