using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Settings;
using GenerativeAI;
using GenerativeAI.Types;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Services
{
    public class EmbeddingService : IEmbeddingService
    {
        private readonly EmbeddingModel _embeddingModel;

        public EmbeddingService (IOptions<GeminiSettings> options)
        {
            var apiKey = options.Value.ApiKey;
            var embeddingModelName = options.Value.EmbeddingModel ?? "text-embedding-004";
            _embeddingModel = new EmbeddingModel(apiKey, embeddingModelName);
        }

        public async Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(text))
                return Array.Empty<float>();

            try
            {
                var response = await _embeddingModel.EmbedContentAsync(
                    new EmbedContentRequest     
                    {
                        Content = new Content
                        {
                            Parts = { new Part { Text = text } }
                        }
                    });

                return response.Embedding?.Values?.Select(v => (float)v).ToArray() ?? Array.Empty<float>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EmbeddingService] Error: {ex.Message}");
                return Array.Empty<float>();
            }
        }
    }
}
