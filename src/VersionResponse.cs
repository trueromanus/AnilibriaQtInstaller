using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Installer {

    /// <summary>
    /// Version response.
    /// </summary>
    public record VersionResponse {

        /// <summary>
        /// Version tag.
        /// </summary>
        [JsonPropertyName ( "tag_name" )]
        public string TagName { get; init; } = "";

        /// <summary>
        /// File assets.
        /// </summary>
        [JsonPropertyName ( "assets" )]
        public IEnumerable<VersionAsset> Assets { get; init; } = new List<VersionAsset> ();

    }

}
