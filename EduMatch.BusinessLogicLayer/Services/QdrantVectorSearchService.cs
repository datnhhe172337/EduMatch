using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Responses;
using EduMatch.DataAccessLayer.Entities;
using Microsoft.Extensions.Configuration;
using Qdrant.Client;
using Qdrant.Client.Grpc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Services
{
    public class QdrantVectorSearchService : IVectorSearchService
    {
        private readonly QdrantClient _client;
        private readonly IEmbeddingService _embeddingService;
        private readonly string _collectionName = "tutors";
        public QdrantVectorSearchService(IConfiguration configuration, IEmbeddingService embeddingService)
        {
            var endpoint = configuration["Qdrant:Endpoint"] ?? "http://localhost:6334";
            _client = new QdrantClient(endpoint);
            _embeddingService = embeddingService;
        }

        public async Task<List<VectorSearchResult>> SearchAsync(float[] queryVector)
        {
            var result = await _client.SearchAsync(_collectionName, queryVector, limit: 5);

            return result.Select(x => new VectorSearchResult
            {
                Id = x.Id.ToString(),
                Score = x.Score,
                Metadata = x.Payload.ContainsKey("tutorInfo")
                    ? x.Payload["tutorInfo"].StringValue
                    : null
            }).ToList();
        }

        public async Task UpsertTutorAsync(TutorProfile tutor)
        {
            // 🧠 1. Tạo embedding từ mô tả hoặc thông tin tổng hợp
            var textToEmbed = $"{tutor.UserEmail}. Bio: {tutor.Bio}. Môn: {tutor.TutorSubjects.ToList()}. Kinh nghiệm: {tutor.TeachingExp}.";
            var embedding = await _embeddingService.GenerateEmbeddingAsync(textToEmbed);

            // 🧩 2. Metadata để lưu kèm vector
            var metadata = new Dictionary<string, Value>
            {
                ["tutorId"] = new Value { StringValue = tutor.Id.ToString() },
                ["name"] = new Value { StringValue = tutor.Name },
                ["subject"] = new Value { StringValue = tutor.Subject },
                ["city"] = new Value { StringValue = tutor.City },
                ["experience"] = new Value { StringValue = tutor.Experience.ToString() }
            };

            // 🧱 3. Gửi lên Qdrant
            var point = new PointStruct
            {
                Id = new PointId { Uuid = Google.Protobuf.ByteString.CopyFromUtf8(tutor.Id.ToString()) },
                Vectors = new Vectors { Vector = { embedding.Select(v => (float)v) } },
                Payload = { metadata }
            };

            await _client.UpsertAsync(_collectionName, new[] { point });

            Console.WriteLine($"✅ Synced Tutor '{tutor.Name}' (ID: {tutor.Id}) into Vector DB.");
        }

        public async Task DeleteTutorAsync(int tutorId)
        {
            await _client.DeleteAsync(_collectionName, new[] { new PointId { Uuid = Google.Protobuf.ByteString.CopyFromUtf8(tutorId.ToString()) } });
            Console.WriteLine($"🗑️ Deleted Tutor (ID: {tutorId}) from Vector DB.");
        }
    }
}
