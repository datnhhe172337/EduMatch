using EduMatch.BusinessLogicLayer.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Requests
{
    public class QdrantHybridSearchRequest
    {
        [JsonPropertyName("vector")]
        public float[] Vector { get; set; }

        [JsonPropertyName("vector_name")]
        public string VectorName { get; set; } = "embedding";

        [JsonPropertyName("query")]
        public string? Query { get; set; }

        [JsonPropertyName("limit")]
        public int Limit { get; set; } = 10;

        [JsonPropertyName("with_payload")]
        public bool WithPayload { get; set; } = true;

        [JsonPropertyName("filter")]
        public QdrantFilter? Filter { get; set; }
    }

    public class QdrantFilter
    {
        [JsonPropertyName("must")]
        public List<QdrantCondition> Must { get; set; } = new();
    }

    public class QdrantCondition
    {
        [JsonPropertyName("key")]
        public string Key { get; set; } = default!;

        [JsonPropertyName("match")]
        public QdrantMatch Match { get; set; } = default!;
    }

    public class QdrantMatch
    {
        [JsonPropertyName("value")]
        public object Value { get; set; } = default!;
    }
}
