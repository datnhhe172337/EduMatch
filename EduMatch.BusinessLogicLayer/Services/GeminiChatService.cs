using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Settings;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static GenerativeAI.VertexAIModels;
using GenerativeAI;
using GenerativeAI.Exceptions;

namespace EduMatch.BusinessLogicLayer.Services
{
    public class GeminiChatService : IGeminiChatService
    {
        private readonly string _apiKey;
        private readonly GenerativeModel _model;
        private const int MaxRetries = 5; // Số lần thử lại tối đa

        public GeminiChatService(IOptions<GeminiSettings> options)
        {
            _apiKey = options.Value.ApiKey;
            var modelName = options.Value.Model ?? "gemini-2.0-flash-lite";
            _model = new GenerativeModel(_apiKey, modelName);

        }

        //public async Task<string> AskAsync(string query)
        //{
        //    var model = new GenerativeModel(_apiKey, "models/gemini-1.5-flash");

        //    var response = await model.GenerateContentAsync(new Content
        //    {
        //        Parts = { new TextPart(query) }
        //    });

        //    return response.Text;
        //}

        public async Task<string> GenerateTextAsync(string prompt)
        {
            for (int attempt = 1; attempt <= MaxRetries; attempt++)
            {
                try
                {
                    var response = await _model.GenerateContentAsync(prompt);
                    return response.Text;
                }
                // Chỉ bắt lỗi API có Code 429 (Resource Exhausted) hoặc 503 (Service Unavailable)
                catch (ApiException ex) when (ex.ErrorCode == 429 || ex.ErrorCode == 503)
                {
                    if (attempt == MaxRetries)
                    {
                        // Lần thử cuối cùng, ném lỗi để ứng dụng xử lý
                        throw;
                    }

                    // Tính toán thời gian chờ: 2^attempt giây (ví dụ: 2s, 4s, 8s, 16s,...)
                    int delayInSeconds = (int)Math.Pow(2, attempt);

                    // In log hoặc thông báo
                    Console.WriteLine(
                        $"Attempt {attempt} failed with Code {ex.ErrorCode}. Retrying in {delayInSeconds} seconds...");

                    // Tạm dừng ứng dụng
                    await Task.Delay(TimeSpan.FromSeconds(delayInSeconds));
                    continue; // Quay lại vòng lặp để thử lại
                }
                catch (Exception)
                {
                    // Bắt các lỗi khác (ví dụ: 400, 404,...) và ném ra
                    throw;
                }
            }
            throw new Exception("Failed to generate content after all retries.");

        }
    }
}
