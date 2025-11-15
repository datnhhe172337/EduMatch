using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Interfaces
{
    public interface IQdrantService
    {
        //Task<List<(int TutorId, double Score, string? TutorInfo)>> HybridSearchAsync(string queryText, float[] vector, int topK = 10, CancellationToken ct = default);
        Task UpsertAsync(int tutorId, float[] vector, string searchText, string tutorInfo, CancellationToken ct = default);
    }
}
