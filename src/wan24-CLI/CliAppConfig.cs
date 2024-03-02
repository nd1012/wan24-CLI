using wan24.Core;

namespace wan24.CLI
{
    /// <summary>
    /// CLI app configuration (<see cref="AppConfig"/>)
    /// </summary>
    public class CliAppConfig : AppConfigBase
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public CliAppConfig() : base() { }

        /// <summary>
        /// Applied CLI app configuration
        /// </summary>
        public static CliAppConfig? AppliedCliConfig { get; protected set; }

        /// <summary>
        /// Background color
        /// </summary>
        public string? BackGroundColor { get; set; }

        /// <summary>
        /// Highlight color
        /// </summary>
        public string? HighlightColor { get; set; }

        /// <summary>
        /// Required color
        /// </summary>
        public string? RequiredColor { get; set; }

        /// <summary>
        /// Optional color
        /// </summary>
        public string? OptionalColor { get; set; }

        /// <summary>
        /// Decoration color
        /// </summary>
        public string? DecorationColor { get; set; }

        /// <summary>
        /// API name color
        /// </summary>
        public string? ApiNameColor { get; set; }

        /// <summary>
        /// API method name color
        /// </summary>
        public string? ApiMethodNameColor { get; set; }

        /// <summary>
        /// Color profiles
        /// </summary>
        public ConsoleColorProfile[]? ColorProfiles { get; set; }

        /// <summary>
        /// Color profile name to use
        /// </summary>
        public string? ColorProfile { get; set; }

        /// <inheritdoc/>
        public override void Apply()
        {
            if (SetApplied)
            {
                if (AppliedCliConfig is not null) throw new InvalidOperationException();
                AppliedCliConfig = this;
            }
            if (ColorProfiles is not null) ConsoleColorProfile.AddRegistered(ColorProfiles);
            if (ColorProfile is not null) ConsoleColorProfile.ApplyRegistered = ColorProfile;
            if (BackGroundColor is not null) CliApiInfo.BackGroundColor = BackGroundColor;
            if (HighlightColor is not null) CliApiInfo.HighlightColor = HighlightColor;
            if (RequiredColor is not null) CliApiInfo.RequiredColor = RequiredColor;
            if (OptionalColor is not null) CliApiInfo.OptionalColor = OptionalColor;
            if (DecorationColor is not null) CliApiInfo.DecorationColor = DecorationColor;
            if (ApiNameColor is not null) CliApiInfo.ApiNameColor = ApiNameColor;
            if (ApiMethodNameColor is not null) CliApiInfo.ApiMethodNameColor = ApiMethodNameColor;
            ApplyProperties(afterBootstrap: false);
            ApplyProperties(afterBootstrap: true);
        }

        /// <inheritdoc/>
        public override async Task ApplyAsync(CancellationToken cancellationToken = default)
        {
            if (SetApplied)
            {
                if (AppliedCliConfig is not null) throw new InvalidOperationException();
                AppliedCliConfig = this;
            }
            if (ColorProfiles is not null) ConsoleColorProfile.AddRegistered(ColorProfiles);
            if (ColorProfile is not null) ConsoleColorProfile.ApplyRegistered = ColorProfile;
            if (BackGroundColor is not null) CliApiInfo.BackGroundColor = BackGroundColor;
            if (HighlightColor is not null) CliApiInfo.HighlightColor = HighlightColor;
            if (RequiredColor is not null) CliApiInfo.RequiredColor = RequiredColor;
            if (OptionalColor is not null) CliApiInfo.OptionalColor = OptionalColor;
            if (DecorationColor is not null) CliApiInfo.DecorationColor = DecorationColor;
            if (ApiNameColor is not null) CliApiInfo.ApiNameColor = ApiNameColor;
            if (ApiMethodNameColor is not null) CliApiInfo.ApiMethodNameColor = ApiMethodNameColor;
            await ApplyPropertiesAsync(afterBootstrap: false, cancellationToken).DynamicContext();
            await ApplyPropertiesAsync(afterBootstrap: true, cancellationToken).DynamicContext();
        }
    }
}
