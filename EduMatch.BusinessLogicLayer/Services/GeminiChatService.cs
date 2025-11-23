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
using GenerativeAI.Types;
using EduMatch.DataAccessLayer.Entities;
using Org.BouncyCastle.Ocsp;
using Microsoft.VisualBasic;

namespace EduMatch.BusinessLogicLayer.Services
{
    public class GeminiChatService : IGeminiChatService
    {
        private readonly string _apiKey;
        private readonly GenerativeModel _model;
        private readonly IChatbotService _chatbotService;

        public GeminiChatService(IOptions<GeminiSettings> options, IChatbotService chatbotService)
        {
            _apiKey = options.Value.ApiKey;
            var modelName = options.Value.Model ?? "gemini-2.0-flash-lite";
            _model = new GenerativeModel(_apiKey, modelName);
            _chatbotService = chatbotService;
        }

        public async Task<string> GenerateTextAsync(int sessionId, string prompt, string userMessage)
        { 
            var fiveLastMessages = await _chatbotService.GetLastMessagesAsync(sessionId, 5);

            var history = BuildGeminiHistory(fiveLastMessages);

            history.Insert(0, new Content
            {
                Role = "user",
                Parts = { new Part { Text = $"[SYSTEM INSTRUCTION]: {prompt}" } }
            });

            // 3. Add message mới từ user
            history.Add(new Content
            {
                Role = "user",
                Parts = { new Part(userMessage) }
            });

            var request = new GenerateContentRequest
            {
                Contents = history
            };

            var response = await _model.GenerateContentAsync(request);

            await _chatbotService.AddMessageAsync(sessionId, "user", userMessage);
            await _chatbotService.AddMessageAsync(sessionId, "assistant", response.Text);
            return response.Text;
        }

        private List<Content> BuildGeminiHistory(List<ChatbotMessage> messages)
        {
            var history = new List<Content>();

            foreach (var msg in messages)
            {
                history.Add(new Content
                {
                    Role = msg.Role == "assistant" ? "model" : msg.Role,
                    Parts = { new Part { Text = msg.Message } }
                });
            }

            return history;
        }



    }
}
