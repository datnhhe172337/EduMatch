using EduMatch.BusinessLogicLayer.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Interfaces
{
    public interface IQdrantService
    {
        Task UpsertTutorAsync(TutorProfileDto tutor);
        Task UpsertTutorsAsync(IEnumerable<TutorProfileDto> tutors);
        Task<List<(TutorProfileDto Tutor, float Score)>> SearchTutorsAsync(float[] queryVector, int topK = 5);
        Task CreateCollectionAsync(string collectionName, int vectorSize);

        Task<List<(TutorProfileDto Tutor, float Score)>> MergeAndRankAsync(
                List<(TutorProfileDto Tutor, float Score)> vectorResults,
                List<(TutorProfileDto Tutor, float Score)> keywordResults,
                int topK = 5);
    }
}
