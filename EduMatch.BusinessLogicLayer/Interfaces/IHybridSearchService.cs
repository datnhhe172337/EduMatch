using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Responses;
using EduMatch.BusinessLogicLayer.Settings;
using EduMatch.DataAccessLayer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Interfaces
{
    public interface IHybridSearchService
    {
        //Task<List<VectorSearchResult>> SearchAsync(float[] queryVector);
        //Task<List<HybridSearchResult>> SearchAsync(string userQuery, CancellationToken ct = default);

        //Task<List<HybridSearchHit>> SearchAsync(
        //        string queryText,
        //        float[] vector,
        //        int topK = 10,
        //        CancellationToken ct = default);

        Task UpsertTutorsAsync(IEnumerable<TutorProfileDto> tutors, CancellationToken ct = default);

        Task<List<HybridSearchHit>> SearchAsync(
        string queryText,
        float[] vector,
        int topK = 10,
        string? subject = null,
        string? province = null,
        string? district = null,
        CancellationToken ct = default);
    }
}
