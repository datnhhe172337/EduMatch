using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Helper
{
    public class TutorPayload
    {
        [JsonPropertyName("tutor_id")]
        public int TutorId { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("short_bio")]
        public string ShortBio { get; set; } = "";

        [JsonPropertyName("subjects")]
        public List<string> Subjects { get; set; } = new();

        [JsonPropertyName("experience_years")]
        public string ExperienceYears { get; set; } = "";

        [JsonPropertyName("teaching_modes")]
        public List<string> TeachingModes { get; set; } = new();

        [JsonPropertyName("rating")]
        public double? Rating { get; set; }

        // BM25 text
        [JsonPropertyName("search_text")]
        public string SearchText { get; set; } = "";

        // Text used to produce embedding (optional, but helpful)
        [JsonPropertyName("embedding_text")]
        public string EmbeddingText { get; set; } = "";
    }
}
