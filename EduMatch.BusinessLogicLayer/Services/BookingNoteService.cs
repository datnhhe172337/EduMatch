using AutoMapper;
using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Requests.BookingNote;
using EduMatch.BusinessLogicLayer.Utils;
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
        private readonly IBookingNoteMediaRepository _bookingNoteMediaRepository;
        private readonly CurrentUserService _currentUserService;
        private readonly IMapper _mapper;

        public BookingNoteService(
            IBookingNoteRepository bookingNoteRepository,
            IBookingRepository bookingRepository,
            IBookingNoteMediaRepository bookingNoteMediaRepository,
            CurrentUserService currentUserService,
            IMapper mapper)
        {
            _bookingNoteRepository = bookingNoteRepository;
            _bookingRepository = bookingRepository;
            _bookingNoteMediaRepository = bookingNoteMediaRepository;
            _currentUserService = currentUserService;
            _mapper = mapper;
        }

        public async Task<BookingNoteDto?> GetByIdAsync(int id)
        {
            if (id <= 0)
                throw new Exception("Id phải lớn hơn 0");

            var entity = await _bookingNoteRepository.GetByIdAsync(id);
            if (entity == null)
                return null;

            entity.BookingNoteMedia = await _bookingNoteMediaRepository.GetByBookingNoteIdAsync(entity.Id);
            return _mapper.Map<BookingNoteDto>(entity);
        }

        public async Task<List<BookingNoteDto>> GetByBookingIdAsync(int bookingId)
        {
            if (bookingId <= 0)
                throw new Exception("BookingId phải lớn hơn 0");

            var entities = await _bookingNoteRepository.GetByBookingIdAsync(bookingId);
            foreach (var note in entities)
            {
                note.BookingNoteMedia = await _bookingNoteMediaRepository.GetByBookingNoteIdAsync(note.Id);
            }
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
                CreatedAt = VietnamTimeProvider.Now(),
                CreatedByEmail = currentEmail
            };

            await _bookingNoteRepository.CreateAsync(entity);
            if (request.Media != null && request.Media.Count > 0)
            {
                var mediaEntities = new List<BookingNoteMedium>();
                foreach (var m in request.Media)
                {
                    mediaEntities.Add(new BookingNoteMedium
                    {
                        BookingNoteId = entity.Id,
                        MediaType = (int)m.MediaType,
                        FileUrl = m.FileUrl.Trim(),
                        FilePublicId = string.IsNullOrWhiteSpace(m.FilePublicId) ? null : m.FilePublicId.Trim(),
                        CreatedAt = VietnamTimeProvider.Now()
                    });
                }

                await _bookingNoteMediaRepository.ReplaceMediaAsync(entity.Id, mediaEntities);
                entity.BookingNoteMedia = mediaEntities;
            }

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
            var updated = await _bookingNoteRepository.UpdateAsync(existing);

            if (request.Media != null)
            {
                var mediaEntities = new List<BookingNoteMedium>();
                foreach (var m in request.Media)
                {
                    mediaEntities.Add(new BookingNoteMedium
                    {
                        BookingNoteId = existing.Id,
                        MediaType = (int)m.MediaType,
                        FileUrl = m.FileUrl.Trim(),
                        FilePublicId = string.IsNullOrWhiteSpace(m.FilePublicId) ? null : m.FilePublicId.Trim(),
                        CreatedAt = VietnamTimeProvider.Now()
                    });
                }

                await _bookingNoteMediaRepository.ReplaceMediaAsync(existing.Id, mediaEntities);
                existing.BookingNoteMedia = mediaEntities;
            }

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
