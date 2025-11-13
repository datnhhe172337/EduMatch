using EduMatch.BusinessLogicLayer.Responses;
using EduMatch.DataAccessLayer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Interfaces
{
    public interface IVectorSearchService
    {
        Task<List<VectorSearchResult>> SearchAsync(float[] queryVector);

        Task UpsertTutorAsync(TutorProfile tutor);
        Task DeleteTutorAsync(int tutorId);
    }
}
