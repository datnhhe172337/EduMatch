using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Settings;
using EduMatch.DataAccessLayer.Enum;
using Google.Protobuf.Collections;
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
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace EduMatch.BusinessLogicLayer.Services
{
    public class QdrantService : IQdrantService
    {
        private readonly QdrantClient _client;
        private readonly IEmbeddingService _embeddingService;
        private readonly string _collectionName = "tutors";

        public QdrantService(IEmbeddingService embeddingService, IOptions<QdrantSettings> options)
        {
            _client = new QdrantClient(
                host: options.Value.Host,
                https: true,
                apiKey: options.Value.ApiKey      
            );
            _embeddingService = embeddingService;
        }

        public async Task CreateCollectionAsync(string name, int vectorSize)
        {
            // Nếu collection đã tồn tại → xóa
            var exists = await _client.CollectionExistsAsync(name);
            if (exists)
            {
                await _client.DeleteCollectionAsync(name);
            }

            // Tạo collection với 1 vector "embedding"
            var vectorParams = new VectorParams
            {
                Size = (uint)vectorSize,
                Distance = Distance.Cosine
            };

            await _client.CreateCollectionAsync(
                collectionName: name,
                vectorsConfig: vectorParams 
            );
        }

       // ----------------------------------------------------------
        // Upsert: Single tutor
        // ----------------------------------------------------------
        public async Task UpsertTutorAsync(TutorProfileDto tutor)
        {
            var point = await BuildPointAsync(tutor);
            await _client.UpsertAsync(_collectionName, new[] { point });
        }

        // ----------------------------------------------------------
        // Upsert: Batch
        // ----------------------------------------------------------
        public async Task UpsertTutorsAsync(IEnumerable<TutorProfileDto> tutors)
        {
            const int batch = 500;

            var buffer = new List<PointStruct>(batch);

            foreach (var t in tutors)
            {
                buffer.Add(await BuildPointAsync(t));

                if (buffer.Count >= batch)
                {
                    await _client.UpsertAsync(_collectionName, buffer);
                    buffer.Clear();
                }
            }

            if (buffer.Count > 0)
                await _client.UpsertAsync(_collectionName, buffer);
        }

        // ----------------------------------------------------------
        // Build PointStruct for GRPC upsert
        // ----------------------------------------------------------
        private async Task<PointStruct> BuildPointAsync(TutorProfileDto tutor)
        {
            string textToEmbed = string.Join(" ",
                 new[]
                 {
                    //$"Tên: {tutor.UserName ?? ""}",
                        //$"Giới tính: {tutor.Gender?.ToString() ?? ""}",
                        //$"Ngày sinh: {tutor.Dob?.ToString("yyyy-MM-dd") ?? ""}",
                        $"Môn học: {string.Join(", ", tutor.TutorSubjects?.Select(s => s.Subject?.SubjectName ?? "") ?? Enumerable.Empty<string>())}",
                        $"Tỉnh/Thành phố: {tutor.Province?.Name ?? ""}",
                        $"Xã/Phường/Khu vực: {tutor.SubDistrict?.Name ?? ""}",
                        $"Lớp: {string.Join(", ", tutor.TutorSubjects?.Select(s => s.Level?.Name ?? "") ?? Enumerable.Empty<string>())}",
                        $"Giá: {string.Join(", ", tutor.TutorSubjects?.Select(s => s.HourlyRate?.ToString() ?? "0") ?? Enumerable.Empty<string>())}",
                        $"Hình thức dạy: {tutor.TeachingModes.ToString() ?? ""}",
                        //$"Tiểu sử: {tutor.Bio ?? ""}",
                        $"Kinh nghiệm: {tutor.TeachingExp ?? ""}"
                 }.Where(s => !string.IsNullOrWhiteSpace(s))
             );

            //Console.WriteLine(textToEmbed);
            var vector = await _embeddingService.GenerateEmbeddingAsync(textToEmbed);
            if (vector == null || vector.Length == 0)
                throw new InvalidOperationException($"Embedding generation failed for tutor {tutor?.Id}");

            var point = new PointStruct
            {
                Id = (ulong)tutor.Id,
                Vectors = vector.ToArray()
            };

            point.Payload.Add("tutorId", new Value { IntegerValue = tutor.Id });
            point.Payload.Add("name", new Value { StringValue = tutor.UserName ?? "" });
            point.Payload.Add("province", new Value { StringValue = tutor.Province?.Name ?? "" });
            point.Payload.Add("subdistrict", new Value { StringValue = tutor.SubDistrict?.Name ?? "" });
            point.Payload.Add("gender", new Value { StringValue = tutor.Gender?.ToString() ?? "" });
            point.Payload.Add("bio", new Value { StringValue = tutor.Bio ?? "" });
            point.Payload.Add("teachingModes", new Value { StringValue = tutor.TeachingModes.ToString() ?? "" });
            point.Payload.Add("experience", new Value { StringValue = tutor.TeachingExp ?? "" });

            // Subjects → ListValue
            if (tutor.TutorSubjects != null)
            {
                var listValue = new ListValue();
                foreach (var s in tutor.TutorSubjects)
                {
                    var structValue = new Struct();
                    structValue.Fields.Add("subjectId", new Value { IntegerValue = s.SubjectId });
                    structValue.Fields.Add("subjectName", new Value { StringValue = s.Subject?.SubjectName ?? "" });
                    structValue.Fields.Add("level", new Value { StringValue = s.Level?.Name ?? "" });
                    structValue.Fields.Add("hourlyRate", new Value { DoubleValue = (double)s.HourlyRate });

                    listValue.Values.Add(new Value { StructValue = structValue });
                }

                point.Payload.Add("subjects", new Value { ListValue = listValue });
            }
            else
            {
                point.Payload.Add("subjects", new Value { NullValue = NullValue.NullValue });
            }

            return point;
        }

        // Convert list → Qdrant JSON payload
        private static Value JsonToValue(List<TutorSubjectDto> list)
        {
            var json = JsonSerializer.Serialize(list);
            return Value.Parser.ParseJson(json);
        }

        // ----------------------------------------------------------
        // Search
        // ----------------------------------------------------------
        public async Task<List<(TutorProfileDto Tutor, float Score)>> SearchTutorsAsync(
            float[] queryVector, int topK = 5)
        {
            var result = await _client.SearchAsync(
                collectionName: _collectionName,
                vector: queryVector,
                limit: (uint)topK
            );
             
            var final = new List<(TutorProfileDto, float)>();

            string GetString(MapField<string, Value> payload, string key)
                => payload.TryGetValue(key, out var val) ? val.StringValue : null;

            foreach (var r in result)
            {
                var p = r.Payload;

                var tutor = new TutorProfileDto
                {
                    Id = (int)r.Id.Num,
                    UserName = GetString(p, "name"),
                    Province = new ProvinceDto { Name = GetString(p, "province") },
                    SubDistrict = new SubDistrictDto { Name = GetString(p, "subdistrict") },
                    TutorSubjects = new List<TutorSubjectDto>(),
                    Bio = GetString(p, "bio"),
                    TeachingExp = GetString(p, "experience"),
                    Gender = Enum.TryParse<Gender>(GetString(p, "gender"), out var g) ? g : null,
                    Dob = DateTime.TryParse(GetString(p, "dob"), out var d) ? d : null,
                    TeachingModes = Enum.TryParse<TeachingMode>(GetString(p, "teachingModes"), out var tm) ? tm : 0,
                };


                // Parse subjects safely
                if (p.TryGetValue("subjects", out var subjectsVal) && subjectsVal.ListValue != null)
                {
                    foreach (var sVal in subjectsVal.ListValue.Values)
                    {
                        if (sVal.StructValue != null)
                        {
                            var structFields = sVal.StructValue.Fields;

                            int subjectId = structFields.TryGetValue("subjectId", out var sid) ? (int)sid.IntegerValue : 0;
                            string subjectName = structFields.TryGetValue("subjectName", out var sname) ? sname.StringValue : null;
                            string levelName = structFields.TryGetValue("level", out var l) ? l.StringValue : null;
                            decimal hourlyRate = structFields.TryGetValue("hourlyRate", out var hr) ? (decimal)hr.DoubleValue : 0;

                            var subject = new TutorSubjectDto
                            {
                                SubjectId = subjectId,
                                Subject = new SubjectDto { SubjectName = subjectName },
                                Level = new LevelDto { Name = levelName },
                                HourlyRate = hourlyRate
                            };

                            tutor.TutorSubjects.Add(subject);
                        }
                    }
                }

                final.Add((tutor, r.Score));
            }

            return final;
        }

        public async Task<List<(TutorProfileDto Tutor, float Score)>> MergeAndRankAsync(List<(TutorProfileDto Tutor, float Score)> vectorResults, List<(TutorProfileDto Tutor, float Score)> keywordResults, int topK = 5)
        {
            var merged = new Dictionary<int, (TutorProfileDto Tutor, float VectorScore, float KeywordScore)>();

            // --- Step 1: Add Vector Results ---
            foreach (var vr in vectorResults)
            {
                merged[vr.Tutor.Id] = (vr.Tutor, vr.Score, 0f);
            }

            // --- Step 2: Add Keyword Results ---
            foreach (var kr in keywordResults)
            {
                if (merged.ContainsKey(kr.Tutor.Id))
                {
                    var e = merged[kr.Tutor.Id];
                    merged[kr.Tutor.Id] = (e.Tutor, e.VectorScore, kr.Score);
                }
                else
                {
                    // tutor found in keyword only
                    merged[kr.Tutor.Id] = (kr.Tutor, 0.05f, kr.Score); // small vector weight
                }
            }

            // --- Step 3: Final Score ---
            var final = merged
                .Select(x =>
                {
                    float vectorScore = x.Value.VectorScore; // [0..1]
                    float keywordScore = Math.Min(1f, x.Value.KeywordScore / 10f); // normalize to [0..1]

                    float finalScore = 0.7f * vectorScore + 0.3f * keywordScore;
                    return (x.Value.Tutor, finalScore);
                })
                .OrderByDescending(x => x.finalScore)
                .Take(topK)
                .ToList();

            return final;
        }
    }
}
