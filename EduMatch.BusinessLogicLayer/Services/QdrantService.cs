using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Settings;
using EduMatch.DataAccessLayer.Enum;
using Microsoft.Extensions.Options;
using Qdrant.Client;
using Qdrant.Client.Grpc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace EduMatch.BusinessLogicLayer.Services
{
    public class QdrantService : IQdrantService
    {
        private readonly HttpClient _httpClient;
        private readonly IEmbeddingService _embeddingService;
        private readonly string _collectionName = "tutors";
        private readonly QdrantSettings _settings;

        public QdrantService(IEmbeddingService embeddingService, IOptions<QdrantSettings> options, IHttpClientFactory httpFactory)
        {
            _httpClient = httpFactory.CreateClient("qdrant");
            _settings = options.Value ?? throw new ArgumentNullException(nameof(options));
            _embeddingService = embeddingService;
        }

        /// <summary>
        /// Upsert single tutor
        /// </summary>
        public async Task UpsertTutorAsync(TutorProfileDto tutor)
        {
            var point = await CreatePointAsync(tutor);

            await UpsertPointsAsync(new[] { point });
        }

        /// <summary>
        /// Batch upsert nhiều tutor
        /// </summary>
        public async Task UpsertTutorsAsync(IEnumerable<TutorProfileDto> tutors)
        {
            var points = new List<object>();

            foreach (var tutor in tutors)
            {
                var point = await CreatePointAsync(tutor);
                points.Add(point);
            }

            await UpsertPointsAsync(points);
        }

        //private async Task UpsertPointsAsync(IEnumerable<object> points)
        //{
        //    // Chuyển sang lists riêng
        //    var ids = new List<ulong>();
        //    var vectors = new List<float[]>();
        //    var payloads = new List<object>();

        //    foreach (dynamic p in points)
        //    {
        //        ids.Add((ulong)p.id);
        //        vectors.Add((float[])p.vector);
        //        payloads.Add(p.payload);
        //    }

        //    // Chia batch <= 500 points mỗi lần
        //    const int batchSize = 500;
        //    for (int i = 0; i < ids.Count; i += batchSize)
        //    {
        //        var batchIds = ids.Skip(i).Take(batchSize).ToList();
        //        var batchVectors = vectors.Skip(i).Take(batchSize).ToList();
        //        var batchPayloads = payloads.Skip(i).Take(batchSize).ToList();

        //        var requestBody = new
        //        {
        //            ids = batchIds,
        //            vectors = batchVectors,
        //            payloads = batchPayloads
        //        };

        //        var response = await _httpClient.PostAsJsonAsync($"/collections/{_collectionName}/points?wait=true", requestBody);

        //        if (!response.IsSuccessStatusCode)
        //        {
        //            var content = await response.Content.ReadAsStringAsync();
        //            throw new InvalidOperationException($"Qdrant returned {response.StatusCode}: {content}");
        //        }
        //    }
        //}

        //private async Task UpsertPointsAsync(IEnumerable<object> points)
        //{
        //    var ids = new List<ulong>();
        //    var vectors = new List<Dictionary<string, float[]>>();
        //    var payloads = new List<object>();

        //    foreach (dynamic p in points)
        //    {
        //        ids.Add((ulong)p.id);

        //        vectors.Add(new Dictionary<string, float[]>
        //{
        //    { "embedding", (float[])p.vector }
        //});

        //        payloads.Add(p.payload);
        //    }

        //    const int batchSize = 500;
        //    for (int i = 0; i < ids.Count; i += batchSize)
        //    {
        //        var body = new
        //        {
        //            points = ids.Skip(i).Take(batchSize)
        //                .Select((id, index) => new
        //                {
        //                    id = id,
        //                    vector = vectors[i + index],
        //                    payload = payloads[i + index]
        //                })
        //        };

        //        var response = await _httpClient.PostAsJsonAsync(
        //            $"/collections/{_collectionName}/points?wait=true", body);

        //        var content = await response.Content.ReadAsStringAsync();

        //        if (!response.IsSuccessStatusCode)
        //            throw new Exception($"Upsert failed: {response.StatusCode}: {content}");
        //    }
        //}

        private async Task UpsertPointsAsync(IEnumerable<object> points)
        {
            const int batchSize = 500;

            var pointList = points.Select(p => new
            {
                id = (ulong)p.GetType().GetProperty("id").GetValue(p),
                vector = (float[])p.GetType().GetProperty("vector").GetValue(p),
                payload = p.GetType().GetProperty("payload").GetValue(p)
            }).ToList();

            for (int i = 0; i < pointList.Count; i += batchSize)
            {
                var batch = pointList.Skip(i).Take(batchSize).ToList();
                var requestBody = new { points = batch };

                var json = JsonSerializer.Serialize(requestBody, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var resp = await _httpClient.PostAsync($"/collections/{_collectionName}/points?wait=true", content);

                if (!resp.IsSuccessStatusCode)
                {
                    var respContent = await resp.Content.ReadAsStringAsync();
                    throw new Exception($"Upsert failed: {resp.StatusCode}: {respContent}");
                }
            }
        }






        //private async Task UpsertPointsAsync(IEnumerable<object> points)
        //{
        //    const int batchSize = 500;

        //    // Tạo list point với đúng format
        //    var pointList = points.Select(p => new
        //    {
        //        id = (ulong)p.GetType().GetProperty("id").GetValue(p),
        //        vector = new { embedding = (float[])p.GetType().GetProperty("vector").GetValue(p) },
        //        payload = p.GetType().GetProperty("payload").GetValue(p)
        //    }).ToList();

        //    for (int i = 0; i < pointList.Count; i += batchSize)
        //    {
        //        var batch = pointList.Skip(i).Take(batchSize).ToList();
        //        var requestBody = new { points = batch };

        //        // Serialize manually để đảm bảo JSON chính xác
        //        var json = JsonSerializer.Serialize(requestBody);
        //        var content = new StringContent(json, Encoding.UTF8, "application/json");

        //        var response = await _httpClient.PostAsync($"/collections/{_collectionName}/points?wait=true", content);
        //        if (!response.IsSuccessStatusCode)
        //        {
        //            var responseContent = await response.Content.ReadAsStringAsync();
        //            throw new InvalidOperationException($"Qdrant returned {response.StatusCode}: {responseContent}");
        //        }
        //    }
        //}


        // <summary>
        /// Tạo PointStruct từ TutorProfileDto
        /// </summary>
        private async Task<object> CreatePointAsync(TutorProfileDto tutor)
        {
            string embeddingText = string.Join(" ",
            new[]
            {
                $"Tên: {tutor.UserName ?? ""}",
                $"Giới tính: {tutor.Gender?.ToString() ?? ""}",
                $"Ngày sinh: {tutor.Dob?.ToString("yyyy-MM-dd") ?? ""}",
                $"Tiểu sử: {tutor.Bio ?? ""}",
                $"Kinh nghiệm: {tutor.TeachingExp ?? ""}",
                $"Môn học: {string.Join(", ", tutor.TutorSubjects?.Select(s => s.Subject?.SubjectName ?? "") ?? Enumerable.Empty<string>())}",
                $"Lớp: {string.Join(", ", tutor.TutorSubjects?.Select(s => s.Level?.Name ?? "") ?? Enumerable.Empty<string>())}",
                $"Giá: {string.Join(", ", tutor.TutorSubjects?.Select(s => s.HourlyRate?.ToString() ?? "0") ?? Enumerable.Empty<string>())}",
                $"Hình thức dạy: {tutor.TeachingModes.ToString() ?? ""}",
                $"Tỉnh/Thành: {tutor.Province?.Name ?? ""}",
                $"Xã: {tutor.SubDistrict?.Name ?? ""}"
            });

            var vector = await _embeddingService.GenerateEmbeddingAsync(embeddingText) ?? Array.Empty<float>();
            if (vector == null || vector.Length == 0)
                throw new InvalidOperationException($"Embedding vector is null or empty for tutor {tutor.Id}");

            var subjects = tutor.TutorSubjects?.Select(ts => new Dictionary<string, object> { ["name"] = ts.Subject?.SubjectName ?? "", ["level"] = ts.Level?.Name ?? "", ["hourlyRate"] = ts.HourlyRate ?? 0 }).ToArray() ?? Array.Empty<object>();

            var payload = new Dictionary<string, object>
            {
                ["tutorId"] = tutor.Id,
                ["name"] = tutor.UserName ?? "",
                ["gender"] = tutor.Gender?.ToString() ?? "",
                ["bio"] = tutor.Bio ?? "",
                ["experience"] = tutor.TeachingExp ?? "",
                ["province"] = tutor.Province?.Name ?? "",
                ["subdistrict"] = tutor.SubDistrict?.Name ?? "",
                ["teachingModes"] = tutor.TeachingModes.ToString() ?? "",
                ["subjects"] = subjects
            };


            return new
            {
                id = (ulong)tutor.Id,
                vector,
                payload
            };
        }

        public async Task<List<(TutorProfileDto Tutor, float Score)>> SearchTutorsAsync(float[] queryVector, int topK = 5)
        {
            var requestBody = new
            {
                vector = new Dictionary<string, float[]>
                {
                    ["embedding"] = queryVector
                },
                limit = topK
            };

            var json = JsonSerializer.Serialize(requestBody, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"/collections/{_collectionName}/points/search", content);

            if (!response.IsSuccessStatusCode)
            {
                var contentResp = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"Qdrant search failed: {response.StatusCode} - {content}");
            }

            var jsonResp = await response.Content.ReadAsStringAsync();
            var searchResult = JsonSerializer.Deserialize<QdrantSearchResult>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (searchResult == null || searchResult.Result.Count == 0) return new List<(TutorProfileDto, float)>();

            var tutors = searchResult.Result.Select(p =>
            {
                var payload = p.Payload;
                var subjects = new List<TutorSubjectDto>();

                if (payload.ContainsKey("subjects") && payload["subjects"] is JsonElement jsonSubjects && jsonSubjects.ValueKind == JsonValueKind.Array)
                {
                    foreach (var sub in jsonSubjects.EnumerateArray())
                    {
                        var name = sub.GetProperty("name").GetString() ?? "";
                        var level = sub.GetProperty("level").GetString() ?? "";
                        var rate = sub.GetProperty("hourlyRate").GetInt32();
                        subjects.Add(new TutorSubjectDto
                        {
                            Subject = new SubjectDto { SubjectName = name },
                            Level = new LevelDto { Name = level },
                            HourlyRate = rate
                        });
                    }
                }

                var tutor = new TutorProfileDto
                {
                    Id = (int)p.Id,
                    UserName = payload.ContainsKey("name") ? payload["name"].ToString() : "",
                    Gender = payload.ContainsKey("gender") && Enum.TryParse<EduMatch.DataAccessLayer.Enum.Gender>(
                         payload["gender"].ToString(), true, out var genderVal)
                            ? genderVal
                            : null,
                    Bio = payload.ContainsKey("bio") ? payload["bio"].ToString() : "",
                    TeachingExp = payload.ContainsKey("experience") ? payload["experience"].ToString() : "",
                    Province = new ProvinceDto { Name = payload.ContainsKey("province") ? payload["province"].ToString() : "" },
                    SubDistrict = new SubDistrictDto { Name = payload.ContainsKey("subdistrict") ? payload["subdistrict"].ToString() : "" },
                    TutorSubjects = subjects
                };

                return (tutor, p.Score);
            }).ToList();

            return tutors;
        }

        public async Task DeleteCollectionAsync(string collectionName)
        {
            var resp = await _httpClient.DeleteAsync($"/collections/{collectionName}?delete_payload=true&delete_vectors=true");
            var content = await resp.Content.ReadAsStringAsync();

            if (!resp.IsSuccessStatusCode)
                throw new Exception($"Delete collection failed: {resp.StatusCode}, {content}");
        }

        public async Task CreateCollectionAsync(string name, int vectorSize)
        {
            var body = new
            {
                vectors = new Dictionary<string, object>
                {
                    ["embedding"] = new
                    {
                        size = vectorSize,
                        distance = "Cosine"
                    }
                }
            };

            var resp = await _httpClient.PutAsJsonAsync($"/collections/{name}", body);
            var json = await resp.Content.ReadAsStringAsync();

            if (!resp.IsSuccessStatusCode)
                throw new Exception($"Create collection failed: {resp.StatusCode}: {json}");
        }



    }
}
