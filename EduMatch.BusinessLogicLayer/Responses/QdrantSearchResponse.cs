using EduMatch.BusinessLogicLayer.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Responses
{
    public class QdrantSearchResponse
    {
        [JsonPropertyName("result")] public QdrantPointResult[]? Result { get; set; }
    }
}
