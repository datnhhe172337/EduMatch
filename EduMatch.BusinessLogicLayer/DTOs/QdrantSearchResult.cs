using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.DTOs
{
    public class QdrantSearchResult
    {
        [JsonPropertyName("result")]
        public List<QdrantPoint> Result { get; set; } = new();
    }
}
