using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Settings;
using Microsoft.Extensions.Options;
using GenerativeAI;
using GenerativeAI.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Services
{
    public class GeminiChatService : IGeminiChatService
    {
        private readonly string _apiKey;

        public GeminiChatService(IOptions<GeminiSettings> options)
        {
            _apiKey = options.Value.ApiKey;
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

        public async Task<string?> AskAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return null;

            try
            {
                var client = new GenerativeModel(_apiKey, "gemini-1.5-flash");

                var response = await client.GenerateContentAsync(query);

                return response.Text;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Gemini Error: {ex.Message}");
                return null;
            }

        }
    }
}
