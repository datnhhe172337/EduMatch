using EduMatch.BusinessLogicLayer.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Interfaces
{
    public interface ILLMRerankService
    {
        //Task<List<(int TutorId, double FinalScore)>> RerankAsync(string userQuery, List<HybridSearchResult> candidates, CancellationToken ct = default);

        Task<List<(int TutorId, double FinalScore)>> RerankAsync(string userQuery, List<HybridSearchHit> candidates, CancellationToken ct = default);
    }
}
