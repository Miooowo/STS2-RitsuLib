using System.Text.Json.Serialization;
using STS2RitsuLib.Utils.Persistence.Migration;

namespace STS2RitsuLib.Data.Models
{
    public sealed class RitsuLibSettings
    {
        public const int CurrentSchemaVersion = 1;

        [JsonPropertyName(ModDataVersion.SchemaVersionProperty)]
        public int SchemaVersion { get; set; } = CurrentSchemaVersion;

        [JsonPropertyName("debug_compatibility_mode")]
        public bool DebugCompatibilityMode { get; set; }
    }
}
