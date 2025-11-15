using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Settings;
using Microsoft.Extensions.Options;
using Qdrant.Client;
using Qdrant.Client.Grpc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Services
{
    public class QdrantService : IQdrantService
    {
        private readonly HttpClient _http;
        private readonly string _collectionName = "tutors";

        public QdrantService(IHttpClientFactory httpFactory)
        {
            _http = httpFactory.CreateClient("qdrant");
        }

        public async Task UpsertAsync(int tutorId, float[] vector, string searchText, string tutorInfo, CancellationToken ct = default)
        {
            var payload = new
            {
                tutorId = tutorId,
                search_text = searchText,
                tutorInfo = tutorInfo
            };

            var body = new
            {
                points = new[]
                {
                new
                {
                    id = tutorId,
                    vector = vector,
                    payload = payload
                }
            }
            };

            var response = await _http.PutAsJsonAsync(
                $"/collections/{_collectionName}/points?wait=true",
                body,
                cancellationToken: ct
            );

            response.EnsureSuccessStatusCode();
        }

        //public async Task UpsertAsync(int tutorId, float[] vector, string searchText, string tutorInfo, CancellationToken ct = default)
        //{
        //    if (vector == null || vector.Length == 0)
        //        throw new ArgumentException("Embedding vector is empty.");

        //    var point = new PointStruct
        //    {
        //        Id = (ulong)tutorId,
        //        Vectors = vector
        //    };

        //    point.Payload["tutorId"] = tutorId;
        //    point.Payload["search_text"] = searchText;
        //    point.Payload["tutorInfo"] = tutorInfo;


        //    await _client.UpsertAsync(_collectionName, new[] { point }, cancellationToken: ct);
        //}


    }
}
