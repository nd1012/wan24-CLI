using wan24.Core;

namespace wan24.CLI
{
    /// <summary>
    /// Console color profile
    /// </summary>
    public record class ConsoleColorProfile
    {
        /// <summary>
        /// Registered profiles (key is the name)
        /// </summary>
        public static readonly Dictionary<string, ConsoleColorProfile> Registered = [];

        /// <summary>
        /// Constructor
        /// </summary>
        public ConsoleColorProfile() { }

        /// <summary>
        /// Apply a registered profile (set to the profile name)
        /// </summary>
        [CliConfig]
        public static string ApplyRegistered
        {
            set
            {
                if (!Registered.TryGetValue(value, out ConsoleColorProfile? profile))
                    throw new ArgumentException("Unknown profile", nameof(value));
                profile.Apply();
            }
        }

        /// <summary>
        /// Profile name
        /// </summary>
        public required string Name { get; init; }

        /// <summary>
        /// Background color
        /// </summary>
        public string BackGroundColor { get; set; } = CliApiInfo.BackGroundColor;

        /// <summary>
        /// Highlight color
        /// </summary>
        public string HighlightColor { get; set; } = CliApiInfo.HighlightColor;

        /// <summary>
        /// Required color
        /// </summary>
        public string RequiredColor { get; set; } = CliApiInfo.RequiredColor;

        /// <summary>
        /// Optional color
        /// </summary>
        public string OptionalColor { get; set; } = CliApiInfo.OptionalColor;

        /// <summary>
        /// Decoration color
        /// </summary>
        public string DecorationColor { get; set; } = CliApiInfo.DecorationColor;

        /// <summary>
        /// API name color
        /// </summary>
        public string ApiNameColor { get; set; } = CliApiInfo.ApiNameColor;

        /// <summary>
        /// API method name color
        /// </summary>
        public string ApiMethodNameColor { get; set; } = CliApiInfo.ApiMethodNameColor;

        /// <summary>
        /// Apply the profile
        /// </summary>
        public virtual void Apply()
        {
            CliApiInfo.BackGroundColor = BackGroundColor;
            CliApiInfo.HighlightColor = HighlightColor;
            CliApiInfo.RequiredColor = RequiredColor;
            CliApiInfo.OptionalColor = OptionalColor;
            CliApiInfo.DecorationColor = DecorationColor;
            CliApiInfo.ApiNameColor = ApiNameColor;
            CliApiInfo.ApiMethodNameColor = ApiMethodNameColor;
        }

        /// <summary>
        /// Add registered profiles
        /// </summary>
        /// <param name="profiles">Profiles</param>
        public static void AddRegistered(params ConsoleColorProfile[] profiles)
        {
            foreach (ConsoleColorProfile profile in profiles)
                Registered[profile.Name] = profile;
        }
    }
}
