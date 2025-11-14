using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.DataAccessLayer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Interfaces
{
    public interface ITutorFeedbackService
    {
        Task<TutorFeedbackDto> CreateFeedbackAsync(CreateTutorFeedbackRequest request, string learnerEmail);
        Task<List<TutorFeedbackDto>> GetFeedbackByLearnerEmailAsync(string learnerEmail);
        Task<List<TutorFeedbackDto>> GetFeedbackByTutorIdAsync(int tutorId);
        Task<List<FeedbackCriterion>> GetAllCriteriaAsync();
        Task<List<TutorFeedbackDto>> GetAllFeedbacksAsync();
        Task<TutorFeedbackDto> GetFeedbackByIdAsync(int feedbackId);
        Task<TutorFeedbackDto> UpdateFeedbackAsync(UpdateTutorFeedbackRequest request, string learnerEmail);
    }

}
