namespace wan24.CLI
{
    /// <summary>
    /// Interface for a type which hosts CLI arguments (the type needs to export a public constructor without any or with DI servable-only parameters; the type can be used as 
    /// an API method parameter type (property type-usage isn't supported))
    /// </summary>
    public interface ICliArguments { }
}
