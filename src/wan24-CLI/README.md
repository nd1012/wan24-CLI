# wan24-CLI

This library contains some CLI app helpers. It uses the 
[Spectre.Console](https://github.com/spectreconsole/spectre.console/tree/main) 
library for console output.

`Spectre.Console` contains some of the functionality this library provides, 
but in a different style, which may not meet some developers needs. The goal 
of this library is to provide a tool for rapid CLI app API development.

## Usage

Create a CLI API type:

```cs
[CliApi]
public class YourCliApi
{
	public YourCliApi() { }

	[CliApi, Required]
	public string Value { get; set; } = null!;

	[CliApi]
	public void YourApiMethod()
	{
		Console.WriteLine(Value);
	}
}
```

In your CLI apps startup:

```cs
await wan24.Core.Bootstrap.Async();
Translation.Current = Translation.Dummy;// Or initialize with your own translations
return await CliApi.RunAsync(args);
```

Usage of your CLI API:

```bash
dotnet app.dll --value 'Hello world!'
```

### CLI argument types

#### Key/value pairs

A key/value pair is indicated with a `--[key]`, followed by its value.

#### Key with multiple values

A key may allow a value list, which follows the key. If a 2nd value starts 
with a dash, it needs to be prepended with another key indicator - example:

```bash
dotnet app.dll --key value1 value2 --key -value3
```

#### Flags

A flag is inidicated with a `-[flag]`, having no value following.

#### Value quoting

A value may be quoted using single or double quotes. If quoted, the value 
needs to be escaped for JSON decoding. A backslash needs double escaping.

#### Supported argument types

Per default these CLR types can be parsed from the CLI argument list:

- `bool`: Flag argument
- `string`: Simple string (key/)value argument
- `string[]`: Simple string (key/)value list argument
- `FileStream`: Simple string (key/)value argument using `CliApiFileStream` 
attribute
- `FileStream[]`: Simple string (key/)value argument using `CliApiFileStream` 
attribute

All other CLR types need to be given as JSON encoded values, or you use a 
custom argument parser - example for float values:

```cs
CliApi.CustomArgumentParsers[typeof(float)] = (name, type, arg, attr) => float.Parse(arg);
```

This custom parser will now be used for `float` argument types. If you want to 
use JSON decoding instead, set the `ParseJson` property value of the `CliApi` 
attribute of the property or method parameter to `true`.

The `CliApiAttribute` allows overriding the `CanParseArgument` property and 
the `ParseArgument` method to implement a custom argument value type parsing 
within the attribute directly.

##### `FileStream` argument value type using `CliApiFileStreamAttribute`

A property or a method parameter type may be `FileStream` or `FileStream[]`, 
if the `CliApiFileStream` attribute was used. The attribute allows to set how 
to create the filestream using `wan24.Core.FsHelper.CreateFileStream`.

The `CliApiFileStreamAttribute` contains a custom argument value parser which 
may also be overridden, if required.

#### Keyless parameters

Example:

```bash
dotnet app.dll -flag value1 --key value2 - value3 value4
```

`-flag` is a flag, while `value1` is a keyless value, and `value2` will be 
stored to the key `key`. `value3` and `value4` are appended to the keyless 
values list of the parsed `CliArguments` instance. The single `dash` is 
used to separate them from the value list for `key`.

Another example:

```bash
dotnet app.dll value1 value2 value3 ... -flag --key valueN
```

All arguments before `-flag` are handled as keyless values.

For binding keyless values to a CLI argument property or parameter, the 
`CliApi` attribute constructor which takes a keyless value offset needs to be 
used (more about that in the following chapters).

#### STDIN and STDOUT

Uing the `StdIn` and `StdOut` attributes you can add STDIN/-OUT usage 
informations to the method help.

### CLI argument binding

#### As API type property

This is the recommended way for the argument binding:

```cs
[CliApi, Required]
public string Value { get; set; } = null!;
```

This binding will set the value of the argument `--value` to the property 
before invoking the requested API method.

Don't worry about the `= null!` initialization of the property: The API 
instance will be validated. If `--value` wasn't given, an usage help will be 
displayed.

#### As API method parameter

```cs
[CliApi]
public void YourApiMethod([CliApi] string value)
{
	...
}
```

This binding will set the value of the argument `--value` to the parameter, 
when invoking the requested API method.

#### As argument object

The argument object:

```cs
public class YourApiMethodArguments : ICliArguments
{
	[CliApi, Required]
	public string Value { get; set; } = null!;
}
```

By implementing the empty `ICliArguments` interface, an argument type won't be 
JSON parsed, but used as argument object.

Don't worry about the `= null!` initialization of the property: The API 
instance will be validated. If `--value` wasn't given, an usage help will be 
displayed.

The API method which will consume the argument object:

```cs
[CliApi]
public void YourApiMethod([CliApi] YourApiMethodArguments args)
{
	...
}
```

### Binding keyless arguments

Example API type:

```cs
[CliApi]
public class YourCliApi
{
	public YourCliApi() { }

	[CliApi(0)]
	public string KeyLessValue1 { get ; set; } = null!;

	[CliApi(1)]
	public string[] KeyLessValues2 { get; set; } = null!;

	[CliApi]
	public string NamedValue { get; set; } = null!;

	[CliApi]
	public void YourApiMethod()
	{
		....
	}
}
```

Example CLI app call:

```bash
dotnet app.dll keyLessValue1 --namedValue 'Named value' keyLessValue2a keyLessValue2b
```

Now the properties of `YourCliApi` have these values:

- `KeyLessValue1`: `"keyLessValue1"`
- `NamedValue`: `"Named value"`
- `KeyLessValues2`: `string[] { "keyLessValue2a", "keyLessValue2b"}`

A keyless value list must always be taken from the last possible offset.

### Multiple API types and multiple methods within an API type

Serving multiple APIs:

```cs
await CliApi.RunAsync(args, default, typeof(YourCliApi1), typeof(YourCliApi2), ...);
```

The first keyless argument needs to be the API name (f.e. `YourApiType`). 
Keyless argument bindings within your APIs still begin with `0`.

When serving multiple API methods within an API type, the second keyless 
argument needs to be the API method name. If you serve only one API type, 
the name of the API method will be taken from the first keyless argument. 
Keyless argument bindings within your APIs still begin with `0`.

### Custom API, method and argument names

You can give a custom API/method/argument name to the `CliApi` attribute 
constructor.

### JSON parsed arguments

Set the `ParseJson` property value of the `CliApi` attribute for an argument 
to `true` to enable JSON parsing of the given value.

**NOTE**: JSON parsing must be enabled for numeric types, for example!

## API documentation

API documentation will be generated automatic and be displayed on wrong usage. 
To add details, you can use the `DisplayText` (for titles) and `Description` 
attributes. If an argument isn't a flag, you can add an example value to 
display to the `Example` property of the `CliApi` attribute.

You may also specify a static string property which returns the help text for 
an API/method/argument by setting the properties namespace and name to the 
`HelpTextProperty` property of the `CliApi` attribute. The text contents will 
be parsed, and `Spectre.Console` markup is supported also. You may use these 
variables:

- `%{CommandLine}`: The command line used to call the CLI in general
- `%{API}`: The current API name
- `%{Method}`: The current API method name

The `wan24-Core` string parser is being used for this.

If your API methods return an exit code, you can add documentation for them 
using the `ExitCode` attribute on the method.

Help output uses the `Spectre.Console` markup syntax for printing rich output 
to an ANSI console. The default colors used can be customized in the static 
`CliApiInfo` properties.

All help output can be localized. For a full localization, you can parse the 
`wan24-CLI` source code with POEdit, for example, too. Parser should look for 
these phrases:

- `_("...")` (single underscore)
- `__("...")` (double underscore)

For intercepting errors there are multiple ways:

### `ICliApiErrorHandler`

If your API type implements the `ICliApiErrorHandler` interface, errors during 
processing will be handled by your API type.

In case your method can't handle the error, you can forward the error handling 
to the default error processing by calling `CliApi.DisplayHelpAsync` and 
setting the value of the parameter `useApi` to `false`.

### `ICliApiHelpProvider`

If your API type implements the `ICliApiHelpProvider` interface, wrong usage 
can be handled by your API type.

**NOTE**: If your API type implements `ICliApiErrorHandler`, too, the help 
provider will only be called, if there was no exception.

In case your method can't display the context help, you can forward the help 
handling to the default help display processing by calling 
`CliApi.DisplayHelpAsync` and setting the value of the parameter `useApi` to 
`false`.

### `ICliApiHelper`

If you create a type which implements the `ICliApiHelper` interface, you can 
set an instance as global help provider to the `CliApi.Helper` property for a 
customized help output in any case.

The used type may implement `ICliApiHelper` and/or `ICliApiHelpProvider`, too.

**NOTE**: Interfaces implemented in an API type will be used in the first 
place! The `CliApi.Helper` instance will only be used, if the API type wasn't 
determined yet, or it doesn't implement error handling / help providing. In 
case your CLI app serves a `CliHelpApi` type, too, it'll be used before 
calling `CliApi.Helper`.

**CAUTION**: Do not call `CliApi.DisplayHelpAsync` from a `ICliApiHelper` 
instance, it may cause an endless loop!

### `CliHelpApi`

Serve the `CliHelpApi` API type for serving help for APIs/methods/arguments:

```bash
# Display a list of possible API names
dotnet app.dll help (-details)

# Display API details
dotnet app.dll help --api [apiName] (-details)

# Display API method details
dotnet app.dll help --api [apiName] --method [methodName] (-details)
```

The optional `-details` flag will force the help API to output more available 
informations.

**TIP**: Serve the `CliHelpApi` as the first (and default) API to display the 
help on any general wrong usage.

## Localization

`wan24-CLI` uses the 
[`wan24-Core`](https://github.com/WAN-Solutions/wan24-Core) localization 
helpers. If you want to localize your CLI API help, you can include the 
`wan24-CLI` source code and match the keyword source `_("...")` to the 
keyword extraction configuration.

All help texts defined as API/method/argument attributes will be translated 
before they're going to be displayed.

## Processing multiple API method calls within one process

Example:

```bash
dotnet app.dll --key value - --key value2 - --key value3
```

A single dash is used to separate API call arguments for one API method call.

How to process the three API method calls:

```cs
await CliApi.RunMultiAsync(args);
```

There are some limitations:

1. API method calls will be processed sequential (not in parallel)
2. The first API method which fails or returns an exit code `!=0` will break 
the processing loop
3. API calls without any argument aren't supported and will be ignored
4. Since a single dash is used as argument separator, it can't be used as 
argument for your APIs

## Dash and double dash handling

A single dash is a nameless flag, while a double dash is a nameless key which 
requires a value to follow.

## Double name apperance handling

A repeated flag will be ignored, while a repeated key which required a value 
creates a value list. An API (method/arguments) name should be unique - 
otherwise the APIs (methods/arguments) would overwrite each other.

## CLI API reflection

```cs
FrozenDictionary<string, CliApiInfo> apiInfos = 
	CliApiInfo.Create(typeof(YourCliApi), typeof(CliHelpApi), ...);
```

The `CliApiInfosExtensions` have some useful helper methods. Using the 
`ReflectionExtensions` and `CliApiContext` methods you may also reflect .NET 
reflection info objects or a CLI API context instance for CLI API object 
detail informations.

## Header output

The `CliApi.GeneralHeader` and `CliApi.HelpHeader` properties store a header, 
which will be displayed in general, or if help is being displayed (if there's 
a general header, the help header will never be displayed).

## Running as a dotnet tool

Since there's no way to determine if the process is running as dotnet tool, 
the CLI command would need to be specified in order to get correct usage 
examples from the CLI help API:

```cs
CliApi.CommandLine = "dotnet tool yourapp";
```

## Best practice

You use this library, 'cause it matches your requirements (which 
`Spectre.Console` alone does not in some cases). You can work work the .NET 
`Console` methods, but since this library references `Spectre.Console` you 
could enrich your CLI app with formatted console output easily, if your app 
runs within an ANSI console.

Tips:

- Create one API type for methods which work with one entity and may share 
argument definitions
- Use API type properties for defining CLI arguments
- Use `ICliArguments` object parameters for encapsulating API method arguments 
within an arguments type, if using API type properties is not an option
- Use data annotations for ensuring valid arguments (the 
[ObjectValidation](https://www.nuget.org/packages/ObjectValidation/) NuGet 
package is being used for deep object validations, if CLI arguments are being 
stored in properties)
