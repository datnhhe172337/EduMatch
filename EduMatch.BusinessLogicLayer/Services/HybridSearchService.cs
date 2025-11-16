using CloudinaryDotNet;
using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Requests;
using EduMatch.BusinessLogicLayer.Responses;
using EduMatch.BusinessLogicLayer.Settings;
using GenerativeAI.Types;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace EduMatch.BusinessLogicLayer.Services
{
    public class HybridSearchService : IHybridSearchService
    {
        private readonly HttpClient _http;
        private readonly string _collectionName = "tutors";
        private readonly QdrantSettings _settings;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly HybridOptions _hybridOpts;
        private readonly IEmbeddingService _embeddingService;

        public HybridSearchService(IHttpClientFactory httpFactory, IOptions<QdrantSettings> options, IOptions<HybridOptions> hybridOpts, IEmbeddingService embeddingService)
        {
            _http = httpFactory.CreateClient("qdrant");
            _settings = options.Value;
            _hybridOpts = hybridOpts.Value;

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = null,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            };
            _embeddingService = embeddingService;
        }

        public async Task EnsureCollectionExistsAsync(CancellationToken ct = default)
        {
            var url = $"{_settings.Endpoint.TrimEnd('/')}/collections/{_collectionName}";
            var resp = await _http.GetAsync(url, ct);

            bool recreate = false;

            if (resp.IsSuccessStatusCode)
            {
                var content = await resp.Content.ReadAsStringAsync(ct);
                using var doc = JsonDocument.Parse(content);
                if (!doc.RootElement.TryGetProperty("vectors", out var vectorsElem) ||
                    !vectorsElem.TryGetProperty("embedding", out var embeddingElem) ||
                    embeddingElem.GetProperty("size").GetInt32() != 768)
                {
                    await _http.DeleteAsync(url, ct);
                    recreate = true;
                }
            }
            else
            {
                recreate = true;
            }

            if (recreate)
            {
                var body = new
                {
                    name = _collectionName,
                    vectors = new
                    {
                        embedding = new
                        {
                            size = 768,
                            distance = "Cosine"
                        }
                    },
                    payload_schema = new
                    {
                        bio_text = "keyword"
                    }
                };

                var json = JsonSerializer.Serialize(body, _jsonOptions);
                using var contentCreate = new StringContent(json, Encoding.UTF8, "application/json");
                var createResp = await _http.PutAsync(url, contentCreate, ct);
                createResp.EnsureSuccessStatusCode();
            }
        }


        public async Task UpsertTutorsAsync(IEnumerable<TutorProfileDto> tutors, CancellationToken ct = default)
        {
            await EnsureCollectionExistsAsync(ct);

            var points = new List<object>();

            foreach (var t in tutors)
            {
                // Tạo text embedding kết hợp nhiều thông tin
                var textForEmbedding = $"{t.UserName} {t.Bio} {string.Join(", ", t.TutorSubjects?.Select(s => s.Subject.SubjectName) ?? Array.Empty<string>())}";
                var vector = await _embeddingService.GenerateEmbeddingAsync(textForEmbedding);

                var payload = new
                {
                    tutorId = t.Id,
                    tutorInfo = t.Bio,
                    userName = t.UserName,
                    subjects = t.TutorSubjects?.Select(s => s.Subject.SubjectName).ToArray(),
                    location = new
                    {
                        province = t.Province?.Name,
                        district = t.SubDistrict?.Name
                    },
                    bio_text = textForEmbedding
                };

                points.Add(new
                {
                    id = t.Id,
                    vector = vector, // trực tiếp array float[]
                    payload
                });

            }

            var body = new { points };
            var url = $"/collections/{_collectionName}/points?wait=true";

            var resp = await _http.PutAsJsonAsync(url, body, ct);
            resp.EnsureSuccessStatusCode();
        }

        public async Task<List<HybridSearchHit>> SearchAsync(
    string queryText,
    float[] vector,
    int topK = 10,
    string? subject = null,
    string? province = null,
    string? district = null,
    CancellationToken ct = default)
        {
            if (vector == null || vector.Length != 768)
                throw new ArgumentException($"Vector must be of length 768. Current length: {vector?.Length ?? 0}");

            await EnsureCollectionExistsAsync(ct);

            // Build filter
            QdrantFilter? filter = null;
            var must = new List<QdrantCondition>();
            if (!string.IsNullOrWhiteSpace(subject))
                must.Add(new QdrantCondition { Key = "subjects", Match = new QdrantMatch { Value = subject } });
            if (!string.IsNullOrWhiteSpace(province))
                must.Add(new QdrantCondition { Key = "location.province", Match = new QdrantMatch { Value = province } });
            if (!string.IsNullOrWhiteSpace(district))
                must.Add(new QdrantCondition { Key = "location.district", Match = new QdrantMatch { Value = district } });
            if (must.Any())
                filter = new QdrantFilter { Must = must };

            // ⚡ Key fix: send vector as array directly, not dictionary
            var req = new
            {
                vector = vector,           // float[] trực tiếp
                vector_name = "embedding", // tên vector collection
                limit = topK,
                with_payload = true,
                filter,
                query = string.IsNullOrWhiteSpace(queryText) ? null : queryText
            };

            var url = $"{_settings.Endpoint.TrimEnd('/')}/collections/{_collectionName}/points/search";
            var json = JsonSerializer.Serialize(req, _jsonOptions);

            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            var resp = await _http.PostAsync(url, content, ct);

            if (!resp.IsSuccessStatusCode)
            {
                var txt = await resp.Content.ReadAsStringAsync(ct);
                throw new HttpRequestException($"Qdrant hybrid_search failed: {resp.StatusCode}: {txt}");
            }

            var respStr = await resp.Content.ReadAsStringAsync(ct);
            var qRes = JsonSerializer.Deserialize<QdrantSearchResponse>(respStr, _jsonOptions);

            var outList = new List<HybridSearchHit>();
            if (qRes?.Result != null)
            {
                foreach (var p in qRes.Result)
                {
                    int tutorId = 0;
                    string tutorInfo = "";

                    if (p.Payload != null)
                    {
                        if (p.Payload.TryGetValue("tutorId", out var idElem))
                            tutorId = idElem.ValueKind == JsonValueKind.Number ? idElem.GetInt32() : int.Parse(idElem.GetRawText().Trim('"'));

                        if (p.Payload.TryGetValue("tutorInfo", out var infoElem))
                            tutorInfo = infoElem.GetString() ?? "";
                    }

                    outList.Add(new HybridSearchHit
                    {
                        TutorId = tutorId,
                        Score = p.Score,
                        VectorScore = p.VectorScore ?? 0,
                        KeywordScore = p.KeywordScore ?? 0,
                        TutorInfo = tutorInfo
                    });
                }
            }

            return outList;
        }

    }
}
