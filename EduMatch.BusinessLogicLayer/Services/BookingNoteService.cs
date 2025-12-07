using AutoMapper;
using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Requests.BookingNote;
using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Services
{
    public class BookingNoteService : IBookingNoteService
    {
        private readonly IBookingNoteRepository _bookingNoteRepository;
        private readonly IBookingRepository _bookingRepository;
        private readonly CurrentUserService _currentUserService;
        private readonly IMapper _mapper;

        public BookingNoteService(
            IBookingNoteRepository bookingNoteRepository,
            IBookingRepository bookingRepository,
            CurrentUserService currentUserService,
            IMapper mapper)
        {
            _bookingNoteRepository = bookingNoteRepository;
            _bookingRepository = bookingRepository;
            _currentUserService = currentUserService;
            _mapper = mapper;
        }

        public async Task<BookingNoteDto?> GetByIdAsync(int id)
        {
            if (id <= 0)
                throw new Exception("Id phải lớn hơn 0");

            var entity = await _bookingNoteRepository.GetByIdAsync(id);
            return entity == null ? null : _mapper.Map<BookingNoteDto>(entity);
        }

        public async Task<List<BookingNoteDto>> GetByBookingIdAsync(int bookingId)
        {
            if (bookingId <= 0)
                throw new Exception("BookingId phải lớn hơn 0");

            var entities = await _bookingNoteRepository.GetByBookingIdAsync(bookingId);
            return _mapper.Map<List<BookingNoteDto>>(entities);
        }

        public async Task<BookingNoteDto> CreateAsync(BookingNoteCreateRequest request)
        {
            if (request == null)
                throw new Exception("Yêu cầu không được để trống");

            if (request.BookingId <= 0)
                throw new Exception("BookingId phải lớn hơn 0");

            if (string.IsNullOrWhiteSpace(request.Content))
                throw new Exception("Content không được để trống");

            // Validate booking exists
            var booking = await _bookingRepository.GetByIdAsync(request.BookingId);
            if (booking == null)
                throw new Exception("Booking không tồn tại");

            EnsureUserIsBookingParticipant(booking);
            var currentEmail = _currentUserService.Email;
            if (string.IsNullOrWhiteSpace(currentEmail))
                throw new UnauthorizedAccessException("User email not found.");

            var entity = new BookingNote
            {
                BookingId = request.BookingId,
                Content = request.Content.Trim(),
                ImageUrl = string.IsNullOrWhiteSpace(request.ImageUrl) ? null : request.ImageUrl.Trim(),
                ImagePublicId = string.IsNullOrWhiteSpace(request.ImagePublicId) ? null : request.ImagePublicId.Trim(),
                VideoUrl = string.IsNullOrWhiteSpace(request.VideoUrl) ? null : request.VideoUrl.Trim(),
                VideoPublicId = string.IsNullOrWhiteSpace(request.VideoPublicId) ? null : request.VideoPublicId.Trim(),
                CreatedAt = DateTime.UtcNow,
                CreatedByEmail = currentEmail
            };

            await _bookingNoteRepository.CreateAsync(entity);
            return _mapper.Map<BookingNoteDto>(entity);
        }

        public async Task<BookingNoteDto?> UpdateAsync(BookingNoteUpdateRequest request)
        {
            if (request == null)
                throw new Exception("Yêu cầu không được để trống");

            if (request.Id <= 0)
                throw new Exception("Id phải lớn hơn 0");

            if (string.IsNullOrWhiteSpace(request.Content))
                throw new Exception("Content không được để trống");

            var existing = await _bookingNoteRepository.GetByIdAsync(request.Id);
            if (existing == null)
                return null;

            var booking = await _bookingRepository.GetByIdAsync(existing.BookingId)
                ?? throw new Exception("Booking không tồn tại");
            EnsureUserIsBookingParticipant(booking);

            existing.Content = request.Content.Trim();
            existing.ImageUrl = string.IsNullOrWhiteSpace(request.ImageUrl) ? null : request.ImageUrl.Trim();
            existing.ImagePublicId = string.IsNullOrWhiteSpace(request.ImagePublicId) ? null : request.ImagePublicId.Trim();
            existing.VideoUrl = string.IsNullOrWhiteSpace(request.VideoUrl) ? null : request.VideoUrl.Trim();
            existing.VideoPublicId = string.IsNullOrWhiteSpace(request.VideoPublicId) ? null : request.VideoPublicId.Trim();

            var updated = await _bookingNoteRepository.UpdateAsync(existing);
            return updated == null ? null : _mapper.Map<BookingNoteDto>(updated);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            if (id <= 0)
                throw new Exception("Id phải lớn hơn 0");

            var note = await _bookingNoteRepository.GetByIdAsync(id);
            if (note == null)
                return false;

            var booking = await _bookingRepository.GetByIdAsync(note.BookingId)
                ?? throw new Exception("Booking không tồn tại");
            EnsureUserIsBookingParticipant(booking);
            EnsureUserIsNoteAuthor(note);

            return await _bookingNoteRepository.DeleteAsync(id);
        }

        private void EnsureUserIsBookingParticipant(Booking booking)
        {
            var currentEmail = _currentUserService.Email;
            if (string.IsNullOrWhiteSpace(currentEmail))
                throw new UnauthorizedAccessException("User email not found.");

            var tutorEmail = booking.TutorSubject?.Tutor?.UserEmail;
            var isParticipant = string.Equals(booking.LearnerEmail, currentEmail, StringComparison.OrdinalIgnoreCase)
                || (tutorEmail != null && string.Equals(tutorEmail, currentEmail, StringComparison.OrdinalIgnoreCase));

            if (!isParticipant)
                throw new UnauthorizedAccessException("Bạn không có quyền thêm/sửa/xóa ghi chú cho booking này.");
        }

        private void EnsureUserIsNoteAuthor(BookingNote note)
        {
            var currentEmail = _currentUserService.Email;
            if (string.IsNullOrWhiteSpace(currentEmail))
                throw new UnauthorizedAccessException("User email not found.");

            if (!string.Equals(note.CreatedByEmail, currentEmail, StringComparison.OrdinalIgnoreCase))
                throw new UnauthorizedAccessException("Chỉ người tạo ghi chú mới được xóa.");
        }
    }
}
