using EduMatch.DataAccessLayer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Interfaces
{
    public interface IGeminiChatService
    {
        Task<string> GenerateTextAsync(int sessionId, string prompt, string userMessage);
    }
}
