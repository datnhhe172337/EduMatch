using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Settings;
using GenerativeAI;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Options;
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

        public async Task<List<HybridSearchHit>> RerankAsync(string userQuery, List<HybridSearchHit> items, CancellationToken ct = default)
        {
            var json = JsonSerializer.Serialize(items);

            var prompt = $@"
                Bạn là hệ thống rerank dựa trên mức độ phù hợp.
                Query: ""{userQuery}""
                Danh sách kết quả hybrid (JSON):
                {json}

                Trả về danh sách đã sắp xếp lại theo độ phù hợp cao nhất.
                Giữ nguyên các trường: TutorId, Score, TutorInfo.
                Không thêm text khác ngoài JSON.";

            var resp = await _model.GenerateContentAsync(prompt);

            var text = resp.Text();
            var reranked = JsonSerializer.Deserialize<List<HybridSearchHit>>(text);

            return reranked ?? items;
        }
    }
}
