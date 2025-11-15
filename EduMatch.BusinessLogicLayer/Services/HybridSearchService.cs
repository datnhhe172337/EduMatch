using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Requests;
using EduMatch.BusinessLogicLayer.Responses;
using EduMatch.BusinessLogicLayer.Settings;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Services
{
    public class HybridSearchService : IHybridSearchService
    {
        private readonly HttpClient _http;
        private readonly string _collectionName = "tutors";
        private readonly QdrantSettings _settings;

        public HybridSearchService(HttpClient httpClient, IOptions<QdrantSettings> options)
        {
            _http = httpClient;
            _settings = options.Value;
        }

        public async Task<List<HybridSearchHit>> SearchAsync(string queryText, float[] vector, int topK = 10, CancellationToken ct = default)
        {
            var url = $"{_settings.Endpoint}/collections/{_collectionName}/points/search";

            var body = new QdrantHybridSearchRequest
            {
                Query = new QdrantQuery { Text = queryText },
                Vector = vector,
                Limit = topK,
                WithPayload = true
            };

            var json = JsonSerializer.Serialize(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var httpRes = await _http.PostAsync(url, content, ct);
            httpRes.EnsureSuccessStatusCode();

            var resStr = await httpRes.Content.ReadAsStringAsync(ct);
            var res = JsonSerializer.Deserialize<QdrantSearchResponse>(resStr);

            var list = new List<HybridSearchHit>();

            if (res?.Result != null)
            {
                foreach (var p in res.Result)
                {
                    int tutorId = 0;
                    string tutorInfo = "";

                    if (p.Payload != null)
                    {
                        if (p.Payload.TryGetValue("tutorId", out var idObj))
                            tutorId = Convert.ToInt32(idObj);

                        if (p.Payload.TryGetValue("tutorInfo", out var tInfo))
                            tutorInfo = tInfo?.ToString() ?? "";
                    }

                    list.Add(new HybridSearchHit
                    {
                        TutorId = tutorId,
                        Score = p.Score,
                        TutorInfo = tutorInfo
                    });
                }
            }

            return list;
        }
    }
}
