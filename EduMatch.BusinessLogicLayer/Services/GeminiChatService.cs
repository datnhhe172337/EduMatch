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

        public GeminiChatService(IOptions<GeminiSettings> options)
        {
            _apiKey = options.Value.ApiKey;
            var modelName = options.Value.Model ?? "gemini-2.0-flash-lite";
            _model = new GenerativeModel(_apiKey, modelName);

        }

        public async Task<string> GenerateTextAsync(string prompt)
        {
            var response = await _model.GenerateContentAsync(prompt);
            return response.Text;
        }
    }
}
