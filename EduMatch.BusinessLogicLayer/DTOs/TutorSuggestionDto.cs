using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.DTOs
{
    public class TutorSuggestionDto
    {
        [JsonPropertyName("rank")] public int Rank { get; set; }
        [JsonPropertyName("tutorId")] public int TutorId { get; set; }
        [JsonPropertyName("name")] public string Name { get; set; }
        [JsonPropertyName("subjects")] public List<string> Subjects { get; set; } = new();
        [JsonPropertyName("levels")] public List<string> Levels { get; set; } = new();
        [JsonPropertyName("province")] public string Province { get; set; }
        [JsonPropertyName("subDistrict")] public string SubDistrict { get; set; }
        [JsonPropertyName("hourlyRates")] public List<decimal> HourlyRates { get; set; } = new();
        [JsonPropertyName("teachingExp")] public string TeachingExp { get; set; }
        [JsonPropertyName("profileUrl")] public string ProfileUrl { get; set; }
        [JsonPropertyName("matchScore")] public float MatchScore { get; set; }
    }
}
