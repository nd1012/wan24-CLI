using System.Reflection;
using wan24.Core;

namespace wan24.CLI
{
    /// <summary>
    /// CLI API context
    /// </summary>
    public sealed class CliApiContext
    {
        /// <summary>
        /// Constructor
        /// </summary>
        internal CliApiContext() { }

        /// <summary>
        /// Exported APIs
        /// </summary>
        public Type[] ExportedApis { get; set; } = null!;

        /// <summary>
        /// API instance
        /// </summary>
        public object? API { get; set; }

        /// <summary>
        /// API method
        /// </summary>
        public MethodInfo? Method { get; set; }

        /// <summary>
        /// API method parameters
        /// </summary>
        public object?[]? Parameters { get; set; }

        /// <summary>
        /// CLI arguments
        /// </summary>
        public CliArguments? Arguments { get; set; }

        /// <summary>
        /// Exception
        /// </summary>
        public Exception? Exception { get; set; }

        /// <summary>
        /// Get the exported API names
        /// </summary>
        /// <returns>Exported API names (sorted ascending)</returns>
        public IEnumerable<string> GetExportedApiNames()
            => from type in ExportedApis
               orderby type.GetCliApiName()
               select type.GetCliApiName();

        /// <summary>
        /// Get the exported API methods
        /// </summary>
        /// <returns>Exported API methods</returns>
        public IEnumerable<MethodInfo> GetExportedApiMethods()
            => API is null ? Array.Empty<MethodInfo>().AsEnumerable() : API.GetType().GetExportedApiMethods();

        /// <summary>
        /// Get the exported API methods
        /// </summary>
        /// <returns>Exported API method names (sorted ascending)</returns>
        public IEnumerable<string> GetExportedApiMethodNames()
            => from mi in GetExportedApiMethods()
               orderby mi.GetCliApiMethodName()
               select mi.GetCliApiMethodName();

        /// <summary>
        /// Get the available arguments
        /// </summary>
        /// <returns>Available arguments (sorted ascending; including dash prefix)</returns>
        public IEnumerable<string> GetAvailableArguments()
            => API is null
                ? Array.Empty<string>().AsEnumerable()
                : API.GetType().GetAvailableArguments(Method);

        /// <summary>
        /// Get an argument host type
        /// </summary>
        /// <param name="arg">Argument (excluding dash prefix)</param>
        /// <returns>Argument host type</returns>
        public CliArgumentHosts GetArgumentHostType(string arg)
            => API is null ? CliArgumentHosts.None : API.GetType().GetArgumentHostType(arg, Method);

        /// <summary>
        /// Get the CLI argument host parameter
        /// </summary>
        /// <param name="arg">Argument (excluding dash prefix)</param>
        /// <returns>Host parameter</returns>
        public ParameterInfo? GetCliArgumentHostParameter(string arg)
            => API?.GetType().GetCliArgumentHostParameter(arg, Method);

        /// <summary>
        /// Get the CLI argument host property
        /// </summary>
        /// <param name="arg">Argument (excluding dash prefix)</param>
        /// <returns>Host property</returns>
        public PropertyInfoExt? GetCliArgumentHostProperty(string arg)
            => API?.GetType().GetCliArgumentHostProperty(arg, Method);
    }
}
