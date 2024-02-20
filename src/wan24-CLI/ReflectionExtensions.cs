using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using wan24.Core;
using wan24.ObjectValidation;

namespace wan24.CLI
{
    /// <summary>
    /// Reflection extensions
    /// </summary>
    public static class ReflectionExtensions
    {
        /// <summary>
        /// Get the CLI API name
        /// </summary>
        /// <param name="type">CLI API type</param>
        /// <param name="attr"><see cref="CliApiAttribute"/></param>
        /// <returns>CLI API name</returns>
        public static string GetCliApiName(this Type type, CliApiAttribute? attr = null)
            => (attr ??= type.GetCustomAttributeCached<CliApiAttribute>()) is not null && attr!.Name.Length != 0 ? attr.Name : type.Name;

        /// <summary>
        /// Get the CLI API name
        /// </summary>
        /// <param name="type">CLI API type</param>
        /// <param name="attr"><see cref="CliApiAttribute"/></param>
        /// <returns>CLI API name</returns>
        public static string GetCliApiName(this Type type, out CliApiAttribute? attr)
            => (attr = type.GetCustomAttributeCached<CliApiAttribute>()) is not null && attr!.Name.Length != 0 ? attr.Name : type.Name;

        /// <summary>
        /// Get the CLI API method name
        /// </summary>
        /// <param name="mi">CLI API method</param>
        /// <param name="attr"><see cref="CliApiAttribute"/></param>
        /// <returns>CLI API method name</returns>
        public static string GetCliApiMethodName(this MethodInfo mi, CliApiAttribute? attr = null)
            => (attr ??= mi.GetCustomAttributeCached<CliApiAttribute>()) is not null && attr!.Name.Length != 0 ? attr.Name : mi.Name;

        /// <summary>
        /// Get the CLI API method name
        /// </summary>
        /// <param name="mi">CLI API method</param>
        /// <param name="attr"><see cref="CliApiAttribute"/></param>
        /// <returns>CLI API method name</returns>
        public static string GetCliApiMethodName(this MethodInfo mi, out CliApiAttribute? attr)
            => (attr = mi.GetCustomAttributeCached<CliApiAttribute>()) is not null && attr!.Name.Length != 0 ? attr.Name : mi.Name;

        /// <summary>
        /// Get the CLI API method name
        /// </summary>
        /// <param name="pi">CLI argument</param>
        /// <param name="attr"><see cref="CliApiAttribute"/></param>
        /// <returns>CLI argument name</returns>
        public static string GetCliApiArgumentName(this PropertyInfo pi, CliApiAttribute? attr = null)
            => (attr ??= pi.GetCustomAttributeCached<CliApiAttribute>()) is not null && attr!.Name.Length != 0 ? attr.Name : pi.Name;

        /// <summary>
        /// Get the CLI API method name
        /// </summary>
        /// <param name="pi">CLI argument</param>
        /// <param name="attr"><see cref="CliApiAttribute"/></param>
        /// <returns>CLI argument name</returns>
        public static string GetCliApiArgumentName(this PropertyInfo pi, out CliApiAttribute? attr)
            => (attr = pi.GetCustomAttributeCached<CliApiAttribute>()) is not null && attr!.Name.Length != 0 ? attr.Name : pi.Name;

        /// <summary>
        /// Get the CLI API method name
        /// </summary>
        /// <param name="pi">CLI argument</param>
        /// <param name="attr"><see cref="CliApiAttribute"/></param>
        /// <returns>CLI argument name</returns>
        public static string GetCliApiArgumentName(this ParameterInfo pi, CliApiAttribute? attr = null)
            => (attr ??= pi.GetCustomAttributeCached<CliApiAttribute>()) is not null && attr!.Name.Length != 0 ? attr.Name : pi.Name!;

        /// <summary>
        /// Get the CLI API method name
        /// </summary>
        /// <param name="pi">CLI argument</param>
        /// <param name="attr"><see cref="CliApiAttribute"/></param>
        /// <returns>CLI argument name</returns>
        public static string GetCliApiArgumentName(this ParameterInfo pi, out CliApiAttribute? attr)
            => (attr = pi.GetCustomAttributeCached<CliApiAttribute>()) is not null && attr!.Name.Length != 0 ? attr.Name : pi.Name!;

        /// <summary>
        /// Get the CLI argument type
        /// </summary>
        /// <param name="type">Argument type</param>
        /// <returns>CLI argument type</returns>
        public static CliArgumentTypes GetCliArgumentType(this Type type)
        {
            if (type == typeof(bool)) return CliArgumentTypes.Flag;
            if (type == typeof(string) || (type.IsArray && type.GetElementType() == typeof(string))) return CliArgumentTypes.Value;
            return CliArgumentTypes.Object;
        }

        /// <summary>
        /// Get the CLI title
        /// </summary>
        /// <param name="type">API type</param>
        /// <returns>CLI title</returns>
        public static string GetCliTitle(this Type type)
            => type.GetCustomAttributeCached<DisplayTextAttribute>()?.DisplayText ?? GetCliApiName(type);

        /// <summary>
        /// Get the CLI description
        /// </summary>
        /// <param name="type">API type</param>
        /// <returns>CLI description</returns>
        public static string? GetCliDescription(this Type type) => type.GetCustomAttributeCached<DescriptionAttribute>()?.Description;

        /// <summary>
        /// Get the CLI title
        /// </summary>
        /// <param name="mi">API method</param>
        /// <returns>CLI title</returns>
        public static string GetCliTitle(this MethodInfo mi)
            => mi.GetCustomAttributeCached<DisplayTextAttribute>()?.DisplayText ?? GetCliApiMethodName(mi);

        /// <summary>
        /// Get the CLI description
        /// </summary>
        /// <param name="mi">API method</param>
        /// <returns>CLI description</returns>
        public static string? GetCliDescription(this MethodInfo mi) => mi.GetCustomAttributeCached<DescriptionAttribute>()?.Description;

        /// <summary>
        /// Get the CLI title
        /// </summary>
        /// <param name="pi">Property</param>
        /// <returns>CLI title</returns>
        public static string GetCliTitle(this PropertyInfo pi)
            => pi.GetCustomAttributeCached<DisplayTextAttribute>()?.DisplayText ?? GetCliApiArgumentName(pi);

        /// <summary>
        /// Get the CLI description
        /// </summary>
        /// <param name="pi">Property</param>
        /// <returns>CLI description</returns>
        public static string? GetCliDescription(this PropertyInfo pi) => pi.GetCustomAttributeCached<DescriptionAttribute>()?.Description;

        /// <summary>
        /// Get the CLI title
        /// </summary>
        /// <param name="pi">Parameter</param>
        /// <returns>CLI title</returns>
        public static string GetCliTitle(this ParameterInfo pi)
            => pi.GetCustomAttributeCached<DisplayTextAttribute>()?.DisplayText ?? GetCliApiArgumentName(pi);

        /// <summary>
        /// Get the CLI description
        /// </summary>
        /// <param name="pi">Parameter</param>
        /// <returns>CLI description</returns>
        public static string? GetCliDescription(this ParameterInfo pi) => pi.GetCustomAttributeCached<DescriptionAttribute>()?.Description;

        /// <summary>
        /// Get the exported API methods
        /// </summary>
        /// <param name="api">API type</param>
        /// <returns>Exported API methods</returns>
        public static IEnumerable<MethodInfo> GetExportedApiMethods(this Type api) => CliApi.FindApiMethods(api);

        /// <summary>
        /// Get the exported API methods
        /// </summary>
        /// <param name="api">API type</param>
        /// <returns>Exported API method names (sorted ascending)</returns>
        public static IEnumerable<string> GetExportedApiMethodNames(this Type api)
            => GetExportedApiMethods(api).Select(mi => mi.GetCliApiMethodName()).OrderBy(name => name);

        /// <summary>
        /// Get the available (named!) arguments
        /// </summary>
        /// <param name="api">API type</param>
        /// <param name="mi">API method</param>
        /// <returns>Available arguments (sorted ascending; including dash prefix)</returns>
        public static IEnumerable<string> GetAvailableArguments(this Type api, MethodInfo? mi = null)
        {
            IEnumerable<string> GetObjectArguments(Type type)
            {
                CliApiAttribute attr;
                foreach (PropertyInfo pi in CliApi.FindApiArguments(type))
                {
                    attr = pi.GetCustomAttributeCached<CliApiAttribute>()!;
                    if (attr.KeyLessOffset == -1)
                    {
                        yield return pi.PropertyType == typeof(bool) ? $"-{pi.GetCliApiArgumentName()}" : $"--{pi.GetCliApiArgumentName()}";
                    }
                    else
                    {
                        yield return attr.KeyLessOffset.ToString();
                    }
                }
            }
            IEnumerable<string> GetArguments()
            {
                foreach (string arg in GetObjectArguments(api))
                    yield return arg;
                if (mi is null) yield break;
                foreach (ParameterInfo pi in mi.GetParameters())
                    if (pi.ParameterType == typeof(bool))
                    {
                        yield return $"-{pi.GetCliApiArgumentName()}";
                    }
                    else if (typeof(ICliArguments).IsAssignableFrom(pi.ParameterType))
                    {
                        foreach (string arg in GetObjectArguments(pi.ParameterType))
                            yield return arg;
                    }
                    else
                    {
                        yield return $"--{pi.GetCliApiArgumentName()}";
                    }
            }
            return GetArguments().Distinct().OrderBy(a => a);
        }

        /// <summary>
        /// Get an argument host type
        /// </summary>
        /// <param name="api">API type</param>
        /// <param name="arg">Argument (excluding dash prefix)</param>
        /// <param name="mi">API method</param>
        /// <returns>Argument host type</returns>
        public static CliArgumentHosts GetArgumentHostType(this Type api, string arg, MethodInfo? mi = null)
        {
            if (CliApi.FindApiArguments(api).Any(pi => pi.Property.GetCliApiArgumentName() == arg)) return CliArgumentHosts.Property;
            if (mi is null) return CliArgumentHosts.None;
            List<Type> seen = [];
            CliArgumentHosts FindArgument(Type type)
            {
                if (seen.Contains(type)) return CliArgumentHosts.None;
                seen.Add(type);
                foreach (PropertyInfoExt pi in CliApi.FindApiArguments(type))
                    if (pi.PropertyType.GetCliArgumentType() == CliArgumentTypes.Object)
                    {
                        if (FindArgument(pi.PropertyType) == CliArgumentHosts.Property) return CliArgumentHosts.Property;
                    }
                    else if (pi.Property.GetCliApiArgumentName() == arg)
                    {
                        return CliArgumentHosts.Property;
                    }
                return CliArgumentHosts.None;
            }
            foreach (ParameterInfo pi in mi.GetParameters())
            {
                if (pi.GetCustomAttributeCached<CliApiAttribute>() is null) continue;
                if (pi.ParameterType.GetCliArgumentType() == CliArgumentTypes.Object && FindArgument(pi.ParameterType) == CliArgumentHosts.Property)
                    return CliArgumentHosts.Property;
                if (pi.GetCliApiArgumentName() == arg) return CliArgumentHosts.Parameter;
            }
            return api.GetCliArgumentHostProperty(arg, mi) is not null ? CliArgumentHosts.Property : CliArgumentHosts.None;
        }

        /// <summary>
        /// Get the CLI argument host parameter
        /// </summary>
        /// <param name="api">API type</param>
        /// <param name="arg">Argument (excluding dash prefix)</param>
        /// <param name="mi">API method</param>
        /// <returns>Host parameter</returns>
        public static ParameterInfo? GetCliArgumentHostParameter(this Type api, string arg, MethodInfo? mi = null)
        {
            if (CliApi.FindApiArguments(api).Any(pi => pi.Property.GetCliApiArgumentName() == arg)) return null;
            if (mi is null) return null;
            List<Type> seen = [];
            CliArgumentHosts FindArgument(Type type)
            {
                if (seen.Contains(type)) return CliArgumentHosts.None;
                seen.Add(type);
                foreach (PropertyInfo pi in CliApi.FindApiArguments(type))
                    if (pi.PropertyType.GetCliArgumentType() == CliArgumentTypes.Object)
                    {
                        if (FindArgument(pi.PropertyType) == CliArgumentHosts.Property) return CliArgumentHosts.Property;
                    }
                    else if (pi.GetCliApiArgumentName() == arg)
                    {
                        return CliArgumentHosts.Property;
                    }
                return CliArgumentHosts.None;
            }
            foreach (ParameterInfo pi in mi.GetParameters())
            {
                if (pi.GetCustomAttributeCached<CliApiAttribute>() is null) continue;
                if (pi.ParameterType.GetCliArgumentType() == CliArgumentTypes.Object && FindArgument(pi.ParameterType) == CliArgumentHosts.Property)
                    return null;
                if (pi.GetCliApiArgumentName() == arg) return pi;
            }
            return null;
        }

        /// <summary>
        /// Get the CLI argument host property
        /// </summary>
        /// <param name="api">API type</param>
        /// <param name="arg">Argument (excluding dash prefix)</param>
        /// <param name="mi">API method</param>
        /// <returns>Host property</returns>
        public static PropertyInfoExt? GetCliArgumentHostProperty(this Type api, string arg, MethodInfo? mi = null)
        {
            if (CliApi.FindApiArguments(api).FirstOrDefault(pi => pi.Property.GetCliApiArgumentName() == arg) is PropertyInfoExt res) return res;
            if (mi is null) return null;
            List<Type> seen = [];
            PropertyInfoExt? FindArgument(Type type)
            {
                if (seen.Contains(type)) return null;
                seen.Add(type);
                foreach (PropertyInfoExt pi in CliApi.FindApiArguments(type))
                    if (pi.PropertyType.GetCliArgumentType() == CliArgumentTypes.Object)
                    {
                        if (FindArgument(pi.PropertyType) is PropertyInfoExt res) return res;
                    }
                    else if (pi.Property.GetCliApiArgumentName() == arg)
                    {
                        return pi;
                    }
                return null;
            }
            foreach (ParameterInfo pi in mi.GetParameters())
            {
                if (pi.GetCustomAttributeCached<CliApiAttribute>() is null) continue;
                if (pi.ParameterType.GetCliArgumentType() != CliArgumentTypes.Object)
                {
                    if (pi.GetCliApiArgumentName() == arg) return null;
                    continue;
                }
                if (FindArgument(pi.ParameterType) is PropertyInfoExt property) return property;
            }
            return null;
        }

        /// <summary>
        /// Find a CLI argument target member by its property/parameter name
        /// </summary>
        /// <param name="api">API type</param>
        /// <param name="member">Member name (of the property or parameter)</param>
        /// <param name="mi">API method</param>
        /// <returns><see cref="PropertyInfo"/> or <see cref="ParameterInfo"/></returns>
        public static object? FindCliArgumentMember(this Type api, string member, MethodInfo? mi = null)
        {
            if (
                ((object?)CliApi.FindApiArguments(api).FirstOrDefault(pi => pi.Name == member)
                    ?? mi?.GetParameters().FirstOrDefault(pi => pi.Name == member && pi.GetCustomAttributeCached<CliApiAttribute>() is not null))
                is object res
                )
                return res;
            if (mi is null) return null;
            List<Type> seen = [];
            PropertyInfo? FindArgument(Type type)
            {
                if (seen.Contains(type)) return null;
                seen.Add(type);
                foreach (PropertyInfo pi in CliApi.FindApiArguments(type))
                    if (pi.PropertyType.GetCliArgumentType() == CliArgumentTypes.Object)
                    {
                        if (FindArgument(pi.PropertyType) is PropertyInfo res) return res;
                    }
                    else if (pi.Name == member)
                    {
                        return pi;
                    }
                return null;
            }
            foreach (ParameterInfo pi in mi.GetParameters())
            {
                if (
                    !typeof(ICliArguments).IsAssignableFrom(pi.ParameterType) ||
                    pi.GetCustomAttributeCached<CliApiAttribute>() is null ||
                    pi.ParameterType.GetCliArgumentType() != CliArgumentTypes.Object
                    )
                    continue;
                if (FindArgument(pi.ParameterType) is PropertyInfo property) return property;
            }
            return null;
        }

        /// <summary>
        /// Is a CLI argument value required?
        /// </summary>
        /// <param name="pi">Property</param>
        /// <param name="nic"><see cref="NullabilityInfoContext"/></param>
        /// <returns>Is required?</returns>
        public static bool IsCliValueRequired(this PropertyInfo pi, NullabilityInfoContext? nic = null) => !pi.IsNullable(nic);

        /// <summary>
        /// Is a CLI argument value required?
        /// </summary>
        /// <param name="pi">Parameter</param>
        /// <param name="nic"><see cref="NullabilityInfoContext"/></param>
        /// <returns>Is required?</returns>
        public static bool IsCliValueRequired(this ParameterInfo pi, NullabilityInfoContext? nic = null) => !pi.HasDefaultValue && !pi.IsNullable(nic);
    }
}
