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
            Dictionary<int, ExitCodeAttribute> exitCodes = [];
            foreach(ExitCodeAttribute exitCode in mi.GetCustomAttributesCached<ExitCodeAttribute>())
                exitCodes[exitCode.Code] = exitCode;
            ExitCodes = exitCodes.ToFrozenDictionary();
            Dictionary<string, CliApiArgumentInfo> args = [];
            string a;
            foreach (string arg in api.Type.GetAvailableArguments(mi))
            {
                a = arg.RemoveDashPrefix();
                switch (api.Type.GetArgumentHostType(a, mi))
                {
                    case CliArgumentHosts.None:
                        break;
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
        public FrozenDictionary<int, ExitCodeAttribute> ExitCodes { get; }

        /// <inheritdoc/>
        public override string ToString()
        {
            StringBuilder sb = new();
            foreach (CliApiArgumentInfo ai in from ai in Parameters.Values
                                              where ai.IsRequired &&
                                                !ai.IsKeyLess
                                              orderby ai.ArgumentName
                                              select ai)
            {
                sb.Append(ai.ToString());
                sb.Append(' ');
            }
            foreach (CliApiArgumentInfo ai in from ai in Parameters.Values
                                              where ai.Type == CliArgumentTypes.Flag
                                              orderby ai.ArgumentName
                                              select ai)
            {
                sb.Append(ai.ToString());
                sb.Append(' ');
            }
            foreach (CliApiArgumentInfo ai in from ai in Parameters.Values
                                              where !ai.IsRequired &&
                                                ai.Type == CliArgumentTypes.Value &&
                                                !ai.IsKeyLess
                                              orderby ai.ArgumentName
                                              select ai)
            {
                sb.Append(ai.ToString());
                sb.Append(' ');
            }
            foreach (CliApiArgumentInfo ai in from ai in Parameters.Values
                                              where ai.IsKeyLess
                                              orderby ai.Attribute.KeyLessOffset
                                              select ai)
            {
                sb.Append(ai.ToString());
                sb.Append(' ');
            }
            if (sb.Length > 0) sb.Remove(sb.Length - 1, 1);
            return sb.ToString();
        }
    }
}
