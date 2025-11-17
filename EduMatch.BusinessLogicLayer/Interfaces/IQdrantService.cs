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
    }
}
