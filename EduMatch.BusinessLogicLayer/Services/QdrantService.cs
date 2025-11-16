using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Settings;
using Microsoft.Extensions.Options;
using Qdrant.Client;
using Qdrant.Client.Grpc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Services
{
    public class QdrantService : IQdrantService
    {
        private readonly QdrantClient _client;
        private readonly IEmbeddingService _embeddingService;
        private readonly string _collectionName = "tutors";
        private readonly QdrantSettings _settings;

        public QdrantService(IEmbeddingService embeddingService, IOptions<QdrantSettings> options)
        {
            _settings = options.Value;
            _client = new QdrantClient(_settings.Endpoint);
            _embeddingService = embeddingService;
        }

        /// <summary>
        /// Upsert single tutor
        /// </summary>
        public async Task UpsertTutorAsync(TutorProfileDto tutor)
        {
            var point = await CreatePointAsync(tutor);

            await _client.UpsertAsync(_collectionName, new List<PointStruct> { point });
        }

        /// <summary>
        /// Batch upsert nhiều tutor
        /// </summary>
        public async Task UpsertTutorsAsync(IEnumerable<TutorProfileDto> tutors)
        {
            var points = new List<PointStruct>();

            foreach (var tutor in tutors)
            {
                var point = await CreatePointAsync(tutor);
                points.Add(point);
            }

            await _client.UpsertAsync(_collectionName, points);
        }

        // <summary>
        /// Tạo PointStruct từ TutorProfileDto
        /// </summary>
        private async Task<PointStruct> CreatePointAsync(TutorProfileDto tutor)
        {
            string embeddingText = string.Join(" ",
                new[]
                {
                    $"Tên: {tutor.UserName ?? "Không rõ"}",
                    $"Giới tính: {tutor.Gender?.ToString() ?? "Không rõ"}",
                    $"Ngày sinh: {tutor.Dob?.ToString("yyyy-MM-dd") ?? "Không rõ"}",
                    $"Tiểu sử: {tutor.Bio ?? "Không có"}",
                    $"Kinh nghiệm: {tutor.TeachingExp ?? "Chưa có"}",
                    $"Môn học: {string.Join(", ", tutor.TutorSubjects?.Select(s => s.Subject.SubjectName) ?? Enumerable.Empty<string>())}",
                    $"Giá: {string.Join(", ", tutor.TutorSubjects?.Select(s => s.HourlyRate) ?? Enumerable.Empty<decimal?>())}",
                    $"Hình thức dạy: {tutor.TeachingModes}",
                    $"Tỉnh/Thành: {tutor.Province?.Name ?? "Không rõ"}",
                    $"Xã: {tutor.SubDistrict?.Name ?? "Không rõ"}"
                }.Where(s => !string.IsNullOrWhiteSpace(s))
            );



            var vector = await _embeddingService.GenerateEmbeddingAsync(embeddingText) ?? Array.Empty<float>(); ;

            var metadata = new Dictionary<string, Value>
            {
                ["tutorId"] = new Value { IntegerValue = tutor.Id },
                ["name"] = new Value { StringValue = tutor.UserName },
                ["gender"] = new Value { StringValue = tutor.Gender.ToString() },
                ["bio"] = new Value { StringValue = tutor.Bio},
                ["experience"] = new Value { StringValue = tutor.TeachingExp },
                ["subjects"] = new Value
                {
                    ListValue = new ListValue
                    {
                        Values =
                        {
                             tutor.TutorSubjects?.Select(ts =>
                                new Value
                                {
                                    StructValue = new Struct
                                    {
                                        Fields =
                                        {
                                            ["name"] = new Value { StringValue = ts.Subject.SubjectName },
                                            ["hourlyRate"] = new Value { DoubleValue = (double)ts.HourlyRate }
                                        }
                                    }
                                }) ?? Enumerable.Empty<Value>()
                        }
                    }
                },
                ["teachingModes"] = new Value { StringValue = tutor.TeachingModes.ToString() },
                ["province"] = new Value { StringValue = tutor.Province?.Name },
                ["subdistrict"] = new Value { StringValue = tutor.SubDistrict?.Name },
            };

            var point = new PointStruct
            {
                Id = (ulong)tutor.Id,      
                Vectors = vector,
                Payload = { metadata }    
            };

            return point;
        }
    }
}
