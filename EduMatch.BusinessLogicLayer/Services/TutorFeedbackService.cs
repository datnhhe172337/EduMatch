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
    public class TutorFeedbackService : ITutorFeedbackService
    {
        private readonly ITutorFeedbackRepository _repo;
        private readonly ITutorRatingSummaryService _summaryService;

        public TutorFeedbackService(ITutorFeedbackRepository repo, ITutorRatingSummaryService summaryService)
        {
            _repo = repo;
            _summaryService = summaryService;
        }

        public async Task<TutorFeedbackDto> CreateFeedbackAsync(CreateTutorFeedbackRequest request, string learnerEmail)
        {

            var existingFeedback = await _repo.GetFeedbackByBookingAsync(request.BookingId, learnerEmail, request.TutorId);
            if (existingFeedback != null)
                throw new InvalidOperationException("Bạn đã đánh giá buổi học này rồi.");

            int completed = await _repo.CountCompletedSessionsAsync(request.BookingId);
            int total = await _repo.GetTotalSessionsAsync(request.BookingId);

            double overall = request.FeedbackDetails.Average(x => x.Rating);

            var feedback = new TutorFeedback
            {
                BookingId = request.BookingId,
                LearnerEmail = learnerEmail,
                TutorId = request.TutorId,
                Comment = request.Comment,
                OverallRating = overall,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null
            };

            await _repo.AddFeedbackAsync(feedback);
            await _repo.SaveAsync();

            var details = request.FeedbackDetails.Select(d => new TutorFeedbackDetail
            {
                FeedbackId = feedback.Id,
                CriterionId = d.CriterionId,
                Rating = d.Rating
            }).ToList();

            await _repo.AddFeedbackDetailRangeAsync(details);
            await _repo.SaveAsync();
            await _summaryService.EnsureAndUpdateSummaryAsync(request.TutorId);
            var savedFeedback = await _repo.GetByIdIncludeDetailsAsync(feedback.Id);

            return new TutorFeedbackDto
            {
                Id = savedFeedback.Id,
                BookingId = savedFeedback.BookingId,
                TutorId = savedFeedback.TutorId,
                LearnerEmail = savedFeedback.LearnerEmail,
                Comment = savedFeedback.Comment,
                OverallRating = savedFeedback.OverallRating,
                CreatedAt = savedFeedback.CreatedAt,
                FeedbackDetails = savedFeedback.TutorFeedbackDetails.Select(d => new TutorFeedbackDetailDto
                {
                    CriterionId = d.CriterionId,
                    CriteriaName = d.Criterion?.Name ?? "(Unknown)",
                    Rating = d.Rating
                }).ToList()
            };
        }

        public async Task<List<FeedbackCriterion>> GetAllCriteriaAsync()
        {
            return await _repo.GetAllCriteriaAsync();
        }

        public async Task<List<TutorFeedbackDto>> GetAllFeedbacksAsync()
        {
            var feedbacks = await _repo.GetAllFeedbacksAsync();

            return feedbacks.Select(f => new TutorFeedbackDto
            {
                Id = f.Id,
                BookingId = f.BookingId,
                TutorId = f.TutorId,
                LearnerEmail = f.LearnerEmail,
                Comment = f.Comment,
                OverallRating = f.OverallRating,
                CreatedAt = f.CreatedAt,
                FeedbackDetails = f.TutorFeedbackDetails.Select(d => new TutorFeedbackDetailDto
                {
                    CriterionId = d.CriterionId,
                    CriteriaName = d.Criterion?.Name ?? "(Unknown)",
                    Rating = d.Rating
                }).ToList()
            }).ToList();
        }

        public async Task<TutorFeedbackDto> GetFeedbackByIdAsync(int feedbackId)
        {
            var feedback = await _repo.GetByIdIncludeDetailsAsync(feedbackId);
            if (feedback == null)
                throw new InvalidOperationException("Không tìm thấy feedback");

            return new TutorFeedbackDto
            {
                Id = feedback.Id,
                BookingId = feedback.BookingId,
                TutorId = feedback.TutorId,
                LearnerEmail = feedback.LearnerEmail,
                Comment = feedback.Comment,
                OverallRating = feedback.OverallRating,
                CreatedAt = feedback.CreatedAt,
                FeedbackDetails = feedback.TutorFeedbackDetails.Select(d => new TutorFeedbackDetailDto
                {
                    CriterionId = d.CriterionId,
                    CriteriaName = d.Criterion?.Name ?? "(Unknown)",
                    Rating = d.Rating
                }).ToList()
            };
        }

        public async Task<List<TutorFeedbackDto>> GetFeedbackByLearnerEmailAsync(string learnerEmail)
        {
            var feedbacks = await _repo.GetFeedbackByLearnerEmailAsync(learnerEmail);

            return feedbacks.Select(f => new TutorFeedbackDto
            {
                Id = f.Id,
                BookingId = f.BookingId,
                TutorId = f.TutorId,
                LearnerEmail = f.LearnerEmail,
                Comment = f.Comment,
                OverallRating = f.OverallRating,
                CreatedAt = f.CreatedAt,
                FeedbackDetails = f.TutorFeedbackDetails.Select(d => new TutorFeedbackDetailDto
                {
                    CriterionId = d.CriterionId,
                    CriteriaName = d.Criterion?.Name ?? "(Unknown)",
                    Rating = d.Rating
                }).ToList()
            }).ToList();
        }

        public async Task<List<TutorFeedbackDto>> GetFeedbackByTutorIdAsync(int tutorId)
        {
            var feedbacks = await _repo.GetFeedbackByTutorIdAsync(tutorId);

            return feedbacks.Select(f => new TutorFeedbackDto
            {
                Id = f.Id,
                BookingId = f.BookingId,
                TutorId = f.TutorId,
                LearnerEmail = f.LearnerEmail,
                Comment = f.Comment,
                OverallRating = f.OverallRating,
                CreatedAt = f.CreatedAt,
                FeedbackDetails = f.TutorFeedbackDetails.Select(d => new TutorFeedbackDetailDto
                {
                    CriterionId = d.CriterionId,
                    CriteriaName = d.Criterion?.Name ?? "(Unknown)",
                    Rating = d.Rating
                }).ToList()
            }).ToList();
        }

        public async Task<TutorFeedbackDto> UpdateFeedbackAsync(UpdateTutorFeedbackRequest request, string learnerEmail)
        {
            var feedback = await _repo.GetFeedbackByBookingAsync(request.BookingId, learnerEmail, request.TutorId);
            if (feedback == null)
                throw new InvalidOperationException("Không tìm thấy feedback");

            if(feedback.UpdatedAt != null || (DateTime.UtcNow - feedback.CreatedAt).TotalHours > 48)
                throw new InvalidOperationException("Bạn không được phép cập nhật feedback nữa");

            feedback.Comment = request.Comment;
            feedback.OverallRating = request.FeedbackDetails.Average(d => d.Rating);
            feedback.UpdatedAt = DateTime.UtcNow;

            var oldDetails = feedback.TutorFeedbackDetails.ToList();

            await _repo.RemoveFeedbackDetailsAsync(oldDetails);

            feedback.TutorFeedbackDetails = request.FeedbackDetails.Select(d => new TutorFeedbackDetail
            {
                CriterionId = d.CriterionId,
                Rating = d.Rating
            }).ToList();

            await _repo.UpdateFeedbackAsync(feedback);
            await _summaryService.EnsureAndUpdateSummaryAsync(request.TutorId);
            return new TutorFeedbackDto
            {
                Id = feedback.Id,
                BookingId = feedback.BookingId,
                TutorId = feedback.TutorId,
                LearnerEmail = feedback.LearnerEmail,
                Comment = feedback.Comment,
                OverallRating = feedback.OverallRating,
                CreatedAt = feedback.CreatedAt,
                UpdateAt = feedback.UpdatedAt,
                FeedbackDetails = feedback.TutorFeedbackDetails.Select(d => new TutorFeedbackDetailDto
                {
                    CriterionId = d.CriterionId,
                    CriteriaName = d.Criterion?.Name ?? "(Unknown)",
                    Rating = d.Rating
                }).ToList()
            };
        }
    }
}
