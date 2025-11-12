using AutoMapper;
using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Services
{
    public class FeedbackService : IFeedbackService
    {
        private readonly IFeedbackRepository _repo;
        private readonly IMapper _mapper;

        public FeedbackService(IFeedbackRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<TutorFeedbackDto> CreateFeedbackAsync(CreateTutorFeedbackRequest request, string learnerEmail)
        {
            // Kiểm tra 80% buổi học
            int completed = await _repo.CountCompletedSessionsAsync(request.BookingId);
            int total = await _repo.GetTotalSessionsAsync(request.BookingId);

            if (total == 0 || (double)completed / total < 0.8)
                throw new Exception("Bạn chỉ có thể đánh giá sau khi hoàn thành ít nhất 80% số buổi học.");

            // Tính điểm trung bình
            double overall = request.FeedbackDetails.Average(x => x.Rating);

            var feedback = new TutorFeedback
            {
                BookingId = request.BookingId,
                LearnerEmail = learnerEmail,
                TutorId = request.TutorId,
                Comment = request.Comment,
                OverallRating = overall
            };

            await _repo.AddFeedbackAsync(feedback);

            var details = request.FeedbackDetails.Select(d => new TutorFeedbackDetail
            {
                FeedbackId = feedback.Id,
                CriterionId = d.CriterionId,
                Rating = d.Rating
            }).ToList();

            await _repo.AddFeedbackDetailRangeAsync(details);

            feedback.TutorFeedbackDetails = details;
            return _mapper.Map<TutorFeedbackDto>(feedback);
        }
    }

}
