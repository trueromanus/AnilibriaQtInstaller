using System.Text.Json.Serialization;

namespace Installer {

    [JsonSerializable ( typeof ( VersionResponse ) )]
    [JsonSerializable ( typeof ( string ) )]
    [JsonSerializable ( typeof ( IEnumerable<VersionAsset> ) )]
    [JsonSerializable ( typeof ( VersionAsset ) )]
    public partial class ResponseContext : JsonSerializerContext {

    }

}
