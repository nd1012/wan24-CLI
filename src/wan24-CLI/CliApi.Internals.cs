using System.Collections.ObjectModel;
using System.Reflection;
using wan24.Core;
using wan24.ObjectValidation;
using static wan24.Core.Logging;

namespace wan24.CLI
{
    // Internals
    public static partial class CliApi
    {
        /// <summary>
        /// Map arguments to an object
        /// </summary>
        /// <param name="obj">Object</param>
        /// <param name="ca">Arguments</param>
        /// <param name="keyLessOffset">Keyless argument offset</param>
        /// <param name="keyLessArgOffset">Current keyless argument offset</param>
        internal static void MapArguments(object obj, CliArguments ca, int keyLessOffset, ref int keyLessArgOffset)
        {
            if (Debug) Logging.WriteDebug($"Mapping arguments to {obj.GetType()} using keyless offset {keyLessOffset} and keyless argument offset {keyLessArgOffset}");
            bool parsed;
            object? value;
            foreach (PropertyInfoExt pi in FindApiArguments(obj.GetType()))
            {
                if (pi.Setter is null) throw new InvalidProgramException($"Argument property {obj.GetType()}.{pi.Name} has no setter");
                if (typeof(ICliArguments).IsAssignableFrom(pi.PropertyType))
                {
                    if (!pi.PropertyType.CanConstruct())
                        throw new InvalidProgramException($"{obj.GetType()}.{pi.Name} property value type {pi.PropertyType} must not be abstract");
                    value = pi.PropertyType.ConstructAuto()
                        ?? throw new InvalidProgramException($"Failed to instance {obj.GetType()}.{pi.Name} parameter value type {pi.PropertyType}");
                    if (Debug) Logging.WriteDebug($"Mapping arguments to host object {pi.PropertyType} (for {obj.GetType()}.{pi.Name})");
                    MapArguments(value, ca, keyLessOffset, ref keyLessArgOffset);
                    pi.Setter(obj, value);
                }
                else
                {
                    if (Debug) Logging.WriteDebug($"Mapping parsed argument {pi.PropertyType} to {obj.GetType()}.{pi.Name}");
                    (parsed, value) = ParseArgument(pi.Name, pi.PropertyType, ca, pi.GetCustomAttributeCached<CliApiAttribute>()!, keyLessOffset, ref keyLessArgOffset);
                    if (parsed)
                    {
                        pi.Setter(obj, value);
                    }
                    else if(Debug)
                    {
                        Logging.WriteDebug($"Argument {pi.PropertyType} for {obj.GetType()}.{pi.Name} couldn't be parsed");
                    }
                }
            }
            obj.ValidateObject(out _);
        }

        /// <summary>
        /// Parse an argument
        /// </summary>
        /// <param name="name">Argument name</param>
        /// <param name="type">Type</param>
        /// <param name="ca">Arguments</param>
        /// <param name="attr">CLI API attribute</param>
        /// <param name="keyLessOffset">Keyless argument offset</param>
        /// <param name="keyLessArgOffset">Current keyless argument offset</param>
        /// <returns>If the value could be parsed, and the parsed value</returns>
        internal static (bool Parsed, object? Value) ParseArgument(string name, Type type, CliArguments ca, CliApiAttribute attr, int keyLessOffset, ref int keyLessArgOffset)
        {
            if (Debug) Logging.WriteDebug($"Parse argument \"{name}\" ({type}) using keyless offset {keyLessOffset} and keyless argument offset {keyLessArgOffset}");
            // Keyless argument
            if (attr.KeyLessOffset != -1)
            {
                bool hasValue = attr.KeyLessOffset < ca.KeyLessArguments.Count - keyLessOffset - keyLessArgOffset;
                if (Debug) Logging.WriteDebug(hasValue ? "Keyless argument value found" : "Keyless argument value not found");
                if (type == typeof(string))
                {
                    // Simple string value
                    if (Debug) Logging.WriteDebug("Simple string value");
                    if (!hasValue) return (false, null);
                    string res = ca.KeyLessArguments[keyLessOffset + keyLessArgOffset];
                    keyLessArgOffset++;
                    return (true, res);
                }
                else if (type.IsArray && type.GetElementType() == typeof(string))
                {
                    // Simple string array
                    if (Debug) Logging.WriteDebug("Simple string array");
                    if (!hasValue) return (false, Array.Empty<string>());
                    string[] res = ca.KeyLessArguments.Skip(keyLessOffset + keyLessArgOffset).ToArray();
                    keyLessArgOffset = ca.KeyLessArguments.Count - keyLessOffset;
                    return (true, res);
                }
                else if (type.IsArray)
                {
                    // Array of JSON parsed values
                    if (Debug) Logging.WriteDebug("Array of JSON parsed values");
                    if (!hasValue) return (false, Array.CreateInstance(type.GetElementType()!, length: 0));
                    string[] values = ca.KeyLessArguments.Skip(keyLessOffset + keyLessArgOffset).ToArray();
                    keyLessArgOffset = ca.KeyLessArguments.Count - keyLessOffset;
                    Array arr = Array.CreateInstance(type.GetElementType()!, values.Length);
                    for (int i = 0, len = values.Length; i < len; arr.SetValue(ParseArgumentJsonValue($"{name}[{i}]", type, values[i], attr), i), i++) ;
                    return (true, arr);
                }
                else
                {
                    // JSON parsed value
                    if (Debug) Logging.WriteDebug("JSON parsed value");
                    if (!hasValue) return (false, null);
                    string value = ca.KeyLessArguments[keyLessOffset + keyLessArgOffset];
                    keyLessArgOffset++;
                    return (true, ParseArgumentJsonValue(name, type, value, attr));
                }
            }
            // Named argument
            string? existingName = ca.GetExistingKey(attr.Name == string.Empty ? name : attr.Name);
            if (existingName is not null && Debug) Logging.WriteDebug($"Using existing argument name \"{existingName}\"");
            if (type == typeof(bool))
            {
                // Flag
                if (Debug) Logging.WriteDebug("Boolean flag");
                if (existingName is not null && !ca.IsBoolean(existingName)) throw new CliArgException($"Argument is a flag (without value)", existingName);
                return (true, existingName is not null && ca[existingName]);
            }
            else if (type == typeof(string))
            {
                // Simple string value
                if (Debug) Logging.WriteDebug("Simple string value");
                if (existingName is null) return (false, null);
                if (ca.IsBoolean(existingName)) throw new CliArgException($"Argument is not a flag (value required)", existingName);
                if (ca.All(existingName).Count != 1)
                    throw new CliArgException($"Only a single value is allowed ({ca.All(existingName).Count} value(e) is/are given)", existingName);
                return (true, ca.Single(existingName));
            }
            else if (type.IsArray && type.GetElementType() == typeof(string))
            {
                // Simple string array
                if (Debug) Logging.WriteDebug("Simple string array");
                if (existingName is null) return (true, Array.Empty<string>());
                if (ca.IsBoolean(existingName)) throw new CliArgException($"Argument is not a flag (value required)", existingName);
                return (true, ca.All(existingName).ToArray());
            }
            else if (type.IsArray)
            {
                // Array of JSON parsed values
                if (Debug) Logging.WriteDebug("Array of parsed JSON values");
                if (existingName is null) return (true, Array.CreateInstance(type.GetElementType()!, length: 0));
                if (ca.IsBoolean(existingName)) throw new CliArgException($"Argument is not a flag (value required)", existingName);
                ReadOnlyCollection<string> values = ca.All(existingName);
                Array res = Array.CreateInstance(type.GetElementType()!, values.Count);
                for (int i = 0, len = values.Count; i < len; res.SetValue(ParseArgumentJsonValue($"{existingName}[{i}]", type, values[i], attr), i), i++) ;
                return (true, res);
            }
            else
            {
                // JSON parsed value
                if (Debug) Logging.WriteDebug("JSON parsed value");
                if (existingName is null) return (false, null);
                if (ca.IsBoolean(existingName)) throw new CliArgException($"Argument is not a flag (value required)", existingName);
                if (ca.All(existingName).Count != 1)
                    throw new CliArgException($"Only a single value is allowed ({ca.All(existingName).Count} value(e) is/are given)", existingName);
                return (true, ParseArgumentJsonValue(existingName, type, ca.Single(existingName), attr));
            }
        }

        /// <summary>
        /// JSON parse an argument value
        /// </summary>
        /// <param name="name">Argument name</param>
        /// <param name="type">Argument type</param>
        /// <param name="arg">Argument value</param>
        /// <param name="attr">CLI API attribute</param>
        /// <returns>Parsed argument value</returns>
        internal static object? ParseArgumentJsonValue(string name, Type type, string arg, CliApiAttribute attr)
            => attr.ParseJson
                ? JsonHelper.DecodeObject(type, arg)
                : throw new InvalidProgramException($"JSON parsing needs to be enabled for argument \"{name}\"");

        /// <summary>
        /// Find all exported CLI APIs
        /// </summary>
        /// <returns>CLI API types</returns>
        internal static Type[] FindExportedApis()
            => (from ass in TypeHelper.Instance.Assemblies
                from type in ass.GetTypes()
                where type.CanConstruct() &&
                  type.GetCustomAttributeCached<CliApiAttribute>() is not null &&
                  type != typeof(CliHelpApi)
                select type).ToArray();

        /// <summary>
        /// Find the default API
        /// </summary>
        /// <param name="types">API types</param>
        /// <returns>Default API type</returns>
        internal static Type? FindDefaultApi(IEnumerable<Type> types)
            => (from type in types
                where type.GetCustomAttributeCached<CliApiAttribute>()?.IsDefault ?? false
                select type).FirstOrDefault();

        /// <summary>
        /// Find API methods
        /// </summary>
        /// <param name="type">API type</param>
        /// <returns>API methods</returns>
        internal static IEnumerable<MethodInfo> FindApiMethods(Type type)
            => from mi in type.GetMethodsCached(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public)
               where mi.GetCustomAttributeCached<CliApiAttribute>() is not null
               select mi;

        /// <summary>
        /// Find the default API method
        /// </summary>
        /// <param name="methods">API methods</param>
        /// <returns>Default API method</returns>
        internal static MethodInfo? FindDefaultApiMethod(IEnumerable<MethodInfo> methods)
            => (from mi in methods
                where mi.GetCustomAttributeCached<CliApiAttribute>()!.IsDefault
                select mi).FirstOrDefault();

        /// <summary>
        /// Find API arguments
        /// </summary>
        /// <param name="type">API type</param>
        /// <returns>API arguments</returns>
        internal static IEnumerable<PropertyInfoExt> FindApiArguments(Type type)
            => from pi in type.GetPropertiesCached(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public)
               where pi.Property.GetCustomAttributeCached<CliApiAttribute>() is not null
               select pi;

        /// <summary>
        /// Find API arguments
        /// </summary>
        /// <param name="mi">API method</param>
        /// <returns>API arguments</returns>
        internal static IEnumerable<ParameterInfo> FindApiArguments(MethodInfo mi)
            => from pi in mi.GetParametersCached()
               where pi.GetCustomAttributeCached<CliApiAttribute>() is not null
               select pi;
    }
}
