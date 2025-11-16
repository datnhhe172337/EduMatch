using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Settings
{
    public class QdrantPointResult
    {
        [JsonPropertyName("id")] public JsonElement? Id { get; set; }
        [JsonPropertyName("score")] public double Score { get; set; }
        [JsonPropertyName("vector_score")] public double? VectorScore { get; set; }
        [JsonPropertyName("keyword_score")] public double? KeywordScore { get; set; }
        [JsonPropertyName("payload")] public Dictionary<string, JsonElement>? Payload { get; set; }
    }
}
