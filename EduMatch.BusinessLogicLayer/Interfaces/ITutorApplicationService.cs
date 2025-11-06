using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Requests;
using EduMatch.DataAccessLayer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Interfaces
{
    public interface ITutorApplicationService
    {
        Task TutorApplyAsync(int classRequestId, string userEmail, string message);

        Task<IEnumerable<TutorApplicationItemDto>> GetTutorApplicationsByClassRequestAsync(int classRequestId, string learnerEmail);

        Task<List<TutorAppliedItemDto?>> GetTutorApplicationsByTutorAsync(string tutorEmail);
        Task<List<TutorAppliedItemDto?>> GetCanceledApplicationsByTutorAsync(string tutorEmail);

        Task EditTutorApplicationAsync(string tutorEmail, TutorApplicationEditRequest request);

        Task CancelApplicationAsync(string tutorEmail, int tutorApplicationId);

    }
}
