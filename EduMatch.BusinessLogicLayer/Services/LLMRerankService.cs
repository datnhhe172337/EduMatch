using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Settings;
using GenerativeAI;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Options;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Services
{
    public class LLMRerankService : ILLMRerankService
    {
        private readonly string _apiKey;
        private readonly GenerativeModel _model;

        public LLMRerankService(IOptions<GeminiSettings> opt)
        {
            _apiKey = opt.Value.ApiKey;
            var modelName = opt.Value.Model ?? "gemini-2.0-flash-lite";
            _model = new GenerativeModel(_apiKey, modelName);
        }

        public async Task<List<(int TutorId, double FinalScore)>> RerankAsync(
    string userQuery,
    List<HybridSearchHit> candidates,
    CancellationToken ct = default)
        {
            if (candidates == null || candidates.Count == 0)
                return new List<(int, double)>();

            // Keep only top 30 to save LLM cost
            var top = candidates
                .OrderByDescending(c => c.Score)
                .Take(30)
                .ToList();

            // Build prompt
            var sb = new StringBuilder();
            sb.AppendLine("You are a ranking assistant. Re-rank the following tutors for the query below.");
            sb.AppendLine($"User Query: {userQuery}");
            sb.AppendLine("Candidates (TutorId | VectorScore | KeywordScore | Snippet):");

            foreach (var c in top)
            {
                sb.AppendLine($"{c.TutorId} | v:{c.VectorScore:F4} | k:{c.KeywordScore:F4} | {c.TutorInfo}");
            }

            sb.AppendLine("Return JSON array sorted by descending final_score. Format:");
            sb.AppendLine("[{\"TutorId\": 12, \"FinalScore\": 0.95}, ...]");

            var prompt = sb.ToString();

            var resp = await _model.GenerateContentAsync(prompt);

            // ============================
            // FIX: Extract text correctly
            // ============================
            string rawText =
                resp.Text ??
                resp.Candidates?.FirstOrDefault()?
                    .Content?.Parts?.OfType<TextPart>()?
                    .FirstOrDefault()?
                    .Text
                ?? "";

            // Try extract JSON array
            var jsonArray = ExtractJsonArray(rawText);
            if (jsonArray != null)
            {
                try
                {
                    var parsed = JsonSerializer.Deserialize<List<RerankOut>>(jsonArray,
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                    if (parsed != null && parsed.Count > 0)
                    {
                        return parsed
                            .Select(p => (p.TutorId, p.FinalScore))
                            .ToList();
                    }
                }
                catch
                {
                    // ignore → fallback below
                }
            }

            // Fallback: deterministic by Qdrant score
            var max = top.Max(t => t.Score);
            var min = top.Min(t => t.Score);

            double Norm(double s) =>
                max == min ? 1.0 : (s - min) / (max - min);

            return top
                .OrderByDescending(t => t.Score)
                .Select(t => (t.TutorId, Norm(t.Score)))
                .ToList();
        }


        private string? ExtractJsonArray(string text)
        {
            var start = text.IndexOf('[');
            var end = text.LastIndexOf(']');
            if (start >= 0 && end > start) return text.Substring(start, end - start + 1);
            return null;
        }

        private class RerankOut { public int TutorId { get; set; } public double FinalScore { get; set; } }

        //public async Task<List<HybridSearchHit>> RerankAsync(string userQuery, List<HybridSearchHit> items, CancellationToken ct = default)
        //{
        //    var json = JsonSerializer.Serialize(items);

        //    var prompt = $@"
        //        Bạn là hệ thống rerank dựa trên mức độ phù hợp.
        //        Query: ""{userQuery}""
        //        Danh sách kết quả hybrid (JSON):
        //        {json}

        //        Trả về danh sách đã sắp xếp lại theo độ phù hợp cao nhất.
        //        Giữ nguyên các trường: TutorId, Score, TutorInfo.
        //        Không thêm text khác ngoài JSON.";

        //    var resp = await _model.GenerateContentAsync(prompt);

        //    var text = resp.Text();
        //    var reranked = JsonSerializer.Deserialize<List<HybridSearchHit>>(text);

        //    return reranked ?? items;
        //}
    }
}
