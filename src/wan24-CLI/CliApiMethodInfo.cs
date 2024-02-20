using System.Collections.Frozen;
using System.Reflection;
using System.Text;
using wan24.Core;

namespace wan24.CLI
{
    /// <summary>
    /// CLI API method informations
    /// </summary>
    public record class CliApiMethodInfo
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="api">API</param>
        /// <param name="mi">API method</param>
        /// <param name="nic"><see cref="NullabilityInfoContext"/></param>
        public CliApiMethodInfo(CliApiInfo api, MethodInfo mi, NullabilityInfoContext nic)
        {
            API = api;
            Method = mi;
            Attribute = mi.GetCustomAttributeCached<CliApiAttribute>();
            Name = mi.GetCliApiMethodName();
            Title = mi.GetCliTitle();
            Description = mi.GetCliDescription();
            Dictionary<string, CliApiArgumentInfo> args = [];
            string a;
            foreach (string arg in api.Type.GetAvailableArguments(mi))
            {
                a = arg.RemoveDashPrefix();
                switch (api.Type.GetArgumentHostType(a, mi))
                {
                    case CliArgumentHosts.Property:
                        {
                            PropertyInfoExt pi = api.Type.GetCliArgumentHostProperty(a, mi) ?? throw new InvalidProgramException($"Failed to get host property for \"{arg}\"");
                            args[a] = pi.GetCustomAttributeCached<CliApiAttribute>()!.GetArgumentInfo(this, pi, nic) ?? new(this, pi, nic);
                        }
                        break;
                    case CliArgumentHosts.Parameter:
                        {
                            ParameterInfo pi = api.Type.GetCliArgumentHostParameter(a, mi) ?? throw new InvalidProgramException($"Failed to get host parameter for \"{arg}\"");
                            args[a] = pi.GetCustomAttributeCached<CliApiAttribute>()!.GetArgumentInfo(this, pi, nic) ?? new(this, pi, nic);
                        }
                        break;
                    default:
                        throw new InvalidProgramException();
                }
            }
            Parameters = args.ToFrozenDictionary();
            Dictionary<int, ExitCodeInfo> exitCodes = [];
            foreach (ExitCodeAttribute exitCode in mi.GetCustomAttributesCached<ExitCodeAttribute>())
                exitCodes[exitCode.Code] = new(api, this, exitCode);
            ExitCodes = exitCodes.ToFrozenDictionary();
            StdIn = mi.GetCustomAttributeCached<StdInAttribute>();
            StdOut = mi.GetCustomAttributeCached<StdOutAttribute>();
        }

        /// <summary>
        /// API
        /// </summary>
        public CliApiInfo API { get; }

        /// <summary>
        /// API method
        /// </summary>
        public MethodInfo Method { get; }

        /// <summary>
        /// <see cref="CliApiAttribute"/>
        /// </summary>
        public CliApiAttribute? Attribute { get; }

        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Parameters (key is the argument name without dash prefix)
        /// </summary>
        public FrozenDictionary<string, CliApiArgumentInfo> Parameters { get; }

        /// <summary>
        /// Title
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// Description
        /// </summary>
        public string? Description { get; }

        /// <summary>
        /// Exit codes
        /// </summary>
        public FrozenDictionary<int, ExitCodeInfo> ExitCodes { get; }

        /// <summary>
        /// STDIN usage attribute
        /// </summary>
        public StdInAttribute? StdIn { get; }

        /// <summary>
        /// STDOUT usage attribute
        /// </summary>
        public StdOutAttribute? StdOut { get; }

        /// <inheritdoc/>
        public override string ToString()
        {
            StringBuilder sb = new();
            // Keyless
            foreach (CliApiArgumentInfo ai in from ai in Parameters.Values
                                              where ai.IsKeyLess
                                              orderby ai.Attribute.KeyLessOffset
                                              select ai)
            {
                sb.Append(ai.ToString());
                sb.Append(' ');
            }
            // Required arguments
            foreach (CliApiArgumentInfo ai in from ai in Parameters.Values
                                              where ai.IsRequired &&
                                                ai.Type != CliArgumentTypes.Flag &&
                                                !ai.IsKeyLess
                                              orderby ai.Name
                                              select ai)
            {
                sb.Append(ai.ToString());
                sb.Append(' ');
            }
            // Flags
            foreach (CliApiArgumentInfo ai in from ai in Parameters.Values
                                              where ai.Type == CliArgumentTypes.Flag
                                              orderby ai.Name
                                              select ai)
            {
                sb.Append(ai.ToString());
                sb.Append(' ');
            }
            // Optional arguments
            foreach (CliApiArgumentInfo ai in from ai in Parameters.Values
                                              where !ai.IsRequired &&
                                                ai.Type != CliArgumentTypes.Flag &&
                                                !ai.IsKeyLess
                                              orderby ai.Name
                                              select ai)
            {
                sb.Append(ai.ToString());
                sb.Append(' ');
            }
            if (Parameters.Count != 0 && sb.Length > 0) sb.Remove(sb.Length - 1, 1);
            // STDIN
            if (StdIn is not null)
                sb.Append(StdIn.Required
                    ? $" [{CliApiInfo.RequiredColor}]< {StdIn.Description}[/]"
                    : $" [{CliApiInfo.DecorationColor}]([/]< [{CliApiInfo.OptionalColor}]{StdIn.Description}[/][{CliApiInfo.DecorationColor}])[/]");
            // STDOUT
            if (StdOut is not null)
                sb.Append(StdOut.Required
                    ? $" [{CliApiInfo.RequiredColor}]< {StdOut.Description}[/]"
                    : $" [{CliApiInfo.DecorationColor}]([/]> [{CliApiInfo.OptionalColor}]{StdOut.Description}[/][{CliApiInfo.DecorationColor}])[/]");
            return sb.ToString();
        }
    }
}
