using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Installer {

    /// <summary>
    /// Version asset.
    /// </summary>
    public record VersionAsset {

        [JsonPropertyName ( "name" )]
        public string Name { get; init; } = "";

        [JsonPropertyName ( "browser_download_url" )]
        public string BrowserDownloadUrl { get; init; } = "";

    }

}
