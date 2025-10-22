using AutoMapper;
using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Requests;
using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Enum;
using EduMatch.DataAccessLayer.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace EduMatch.BusinessLogicLayer.Services
{
    public class TutorEducationService : ITutorEducationService
    {
        private readonly ITutorEducationRepository _repository;
        private readonly IEducationInstitutionRepository _educationInstitutionRepository;
        private readonly ITutorProfileRepository _tutorProfileRepository;
        private readonly IMapper _mapper;
        private readonly ICloudMediaService _cloudMedia;
        private readonly CurrentUserService _currentUserService;

        public TutorEducationService(
            ITutorEducationRepository repository,
            IMapper mapper,
            ICloudMediaService cloudMedia,
            CurrentUserService currentUserService,
            ITutorProfileRepository tutorProfileRepository,
            IEducationInstitutionRepository educationInstitutionRepository
            )
        {
            _repository = repository;
            _mapper = mapper;
            _cloudMedia = cloudMedia;
            _currentUserService = currentUserService;
            _tutorProfileRepository = tutorProfileRepository;
            _educationInstitutionRepository = educationInstitutionRepository;
        }

        public async Task<TutorEducationDto?> GetByIdFullAsync(int id)
        {
            var entity = await _repository.GetByIdFullAsync(id);
            return entity != null ? _mapper.Map<TutorEducationDto>(entity) : null;
        }

        public async Task<TutorEducationDto?> GetByTutorIdFullAsync(int tutorId)
        {
            var entity = await _repository.GetByTutorIdFullAsync(tutorId);
            return entity != null ? _mapper.Map<TutorEducationDto>(entity) : null;
        }

        public async Task<IReadOnlyList<TutorEducationDto>> GetByTutorIdAsync(int tutorId)
        {
            var entities = await _repository.GetByTutorIdAsync(tutorId);
            return _mapper.Map<IReadOnlyList<TutorEducationDto>>(entities);
        }

        public async Task<IReadOnlyList<TutorEducationDto>> GetByInstitutionIdAsync(int institutionId)
        {
            var entities = await _repository.GetByInstitutionIdAsync(institutionId);
            return _mapper.Map<IReadOnlyList<TutorEducationDto>>(entities);
        }

        public async Task<IReadOnlyList<TutorEducationDto>> GetByVerifiedStatusAsync(VerifyStatus verified)
        {
            var entities = await _repository.GetByVerifiedStatusAsync(verified);
            return _mapper.Map<IReadOnlyList<TutorEducationDto>>(entities);
        }

        public async Task<IReadOnlyList<TutorEducationDto>> GetPendingVerificationsAsync()
        {
            var entities = await _repository.GetPendingVerificationsAsync();
            return _mapper.Map<IReadOnlyList<TutorEducationDto>>(entities);
        }

        public async Task<IReadOnlyList<TutorEducationDto>> GetAllFullAsync()
        {
            var entities = await _repository.GetAllFullAsync();
            return _mapper.Map<IReadOnlyList<TutorEducationDto>>(entities);
        }

        public async Task<TutorEducationDto> CreateAsync(TutorEducationCreateRequest request)
        {
            try
            {
                var tutor = await _tutorProfileRepository.GetByIdFullAsync(request.TutorId);
                if (tutor is null)
                    throw new ArgumentException($"Tutor with ID {request.TutorId} not found.");

                var institution = await _educationInstitutionRepository.GetByIdAsync(request.InstitutionId);
                if (institution is null)
                    throw new ArgumentException($"Education institution with ID {request.InstitutionId} not found.");

                if (_currentUserService.Email is null)
                    throw new ArgumentException("Current user email not found.");

                string? certUrl = null;
                string? certPublicId = null;
                var hasFile = request.CertificateEducation != null && request.CertificateEducation.Length > 0 && !string.IsNullOrWhiteSpace(request.CertificateEducation.FileName);
                if (hasFile)
                {
                    using var stream = request.CertificateEducation!.OpenReadStream();
                    var uploadRequest = new UploadToCloudRequest(
                        Content: stream,
                        FileName: request.CertificateEducation!.FileName,
                        ContentType: request.CertificateEducation!.ContentType ?? "application/octet-stream",
                        LengthBytes: request.CertificateEducation!.Length,
                        OwnerEmail: _currentUserService.Email!,
                        MediaType: MediaType.Image
                    );
                    var uploadResult = await _cloudMedia.UploadAsync(uploadRequest);
                    if (!uploadResult.Ok || string.IsNullOrEmpty(uploadResult.SecureUrl))
                        throw new InvalidOperationException($"Failed to upload file: {uploadResult.ErrorMessage}");
                    certUrl = uploadResult.SecureUrl;
                    certPublicId = uploadResult.PublicId;
                }

                // Map from DTO, then set non-DTO properties
                var entity = _mapper.Map<TutorEducation>(request);
                entity.CertificateUrl = certUrl;
                entity.CertificatePublicId = certPublicId;
                entity.CreatedAt = DateTime.UtcNow;
                entity.Verified = VerifyStatus.Pending;
                entity.RejectReason = null;

                await _repository.AddAsync(entity);
                return _mapper.Map<TutorEducationDto>(entity);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to create tutor education: {ex.Message}", ex);
            }
        }

        public async Task<TutorEducationDto> UpdateAsync(TutorEducationUpdateRequest request)
        {
            try
            {
                var existingEntity = await _repository.GetByIdFullAsync(request.Id);
                if (existingEntity == null)
                {
                    throw new ArgumentException($"Tutor education with ID {request.Id} not found");
                }

                var oldPublicId = existingEntity.CertificatePublicId;
                var hasNewFile = request.CertificateEducation != null && request.CertificateEducation.Length > 0;

                if (hasNewFile)
                {
                    using var stream = request.CertificateEducation!.OpenReadStream();
                    var uploadRequest = new UploadToCloudRequest(
                        Content: stream,
                        FileName: request.CertificateEducation!.FileName,
                        ContentType: request.CertificateEducation!.ContentType ?? "application/octet-stream",
                        LengthBytes: request.CertificateEducation!.Length,
                        OwnerEmail: _currentUserService.Email!,
                        MediaType: MediaType.Image
                    );
                    var uploadResult = await _cloudMedia.UploadAsync(uploadRequest);
                    if (!uploadResult.Ok) throw new InvalidOperationException($"Failed to upload file: {uploadResult.ErrorMessage}");
                    existingEntity.CertificateUrl = uploadResult.SecureUrl;
                    existingEntity.CertificatePublicId = uploadResult.PublicId;
                }

                // Map admin-level properties
                existingEntity.TutorId = request.TutorId;
                existingEntity.InstitutionId = request.InstitutionId;
                if (request.IssueDate.HasValue) existingEntity.IssueDate = request.IssueDate.Value;

                // --- THIS IS THE ADMIN LOGIC ---
                if (request.Verified.HasValue)
                    existingEntity.Verified = request.Verified.Value;

                if (existingEntity.Verified == VerifyStatus.Rejected)
                {
                    if (string.IsNullOrWhiteSpace(request.RejectReason))
                        throw new ArgumentException("Reject reason is required when verification status is Rejected.");
                    existingEntity.RejectReason = request.RejectReason!.Trim();
                }
                else
                {
                    // Clear reject reason if not rejected
                    existingEntity.RejectReason = null;
                }
                // --- END ADMIN LOGIC ---

                await _repository.UpdateAsync(existingEntity);

                if (hasNewFile && !string.IsNullOrWhiteSpace(oldPublicId))
                {
                    _ = _cloudMedia.DeleteByPublicIdAsync(oldPublicId, MediaType.Image);
                }

                return _mapper.Map<TutorEducationDto>(existingEntity);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to update tutor education: {ex.Message}", ex);
            }
        }

        // --- REPLACED this method to use the new DTO and logic ---
        public async Task<TutorEducationDto> UpdateAsync(UpdateTutorEducationRequest request)
        {
            try
            {
                var existingEntity = await _repository.GetByIdFullAsync(request.Id);
                if (existingEntity == null)
                {
                    throw new ArgumentException($"Tutor education with ID {request.Id} not found");
                }

                var oldPublicId = existingEntity.CertificatePublicId;
                var hasNewFile = request.CertificateEducation != null && request.CertificateEducation.Length > 0;

                if (hasNewFile)
                {
                    // Your existing upload logic
                    using var stream = request.CertificateEducation!.OpenReadStream();
                    var uploadRequest = new UploadToCloudRequest(
                        Content: stream,
                        FileName: request.CertificateEducation!.FileName,
                        ContentType: request.CertificateEducation!.ContentType ?? "application/octet-stream",
                        LengthBytes: request.CertificateEducation!.Length,
                        OwnerEmail: _currentUserService.Email!,
                        MediaType: MediaType.Image
                    );
                    var uploadResult = await _cloudMedia.UploadAsync(uploadRequest);
                    if (!uploadResult.Ok || string.IsNullOrEmpty(uploadResult.SecureUrl))
                        throw new InvalidOperationException($"Failed to upload file: {uploadResult.ErrorMessage}");

                    existingEntity.CertificateUrl = uploadResult.SecureUrl;
                    existingEntity.CertificatePublicId = uploadResult.PublicId;
                }
                else if (request.RemoveCertificate && !string.IsNullOrWhiteSpace(oldPublicId))
                {
                    // Logic to remove the file
                    existingEntity.CertificateUrl = null;
                    existingEntity.CertificatePublicId = null;
                }

                // Map other properties from the DTO
                existingEntity.InstitutionId = request.InstitutionId;
                if (request.IssueDate.HasValue)
                    existingEntity.IssueDate = request.IssueDate.Value;

                // When a user updates, reset verification status
                existingEntity.Verified = VerifyStatus.Pending;
                existingEntity.RejectReason = null;

                await _repository.UpdateAsync(existingEntity);

                // Delete the old file *after* DB update is successful
                if ((hasNewFile || request.RemoveCertificate) && !string.IsNullOrWhiteSpace(oldPublicId))
                {
                    _ = _cloudMedia.DeleteByPublicIdAsync(oldPublicId, MediaType.Image);
                }

                return _mapper.Map<TutorEducationDto>(existingEntity);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to update tutor education: {ex.Message}", ex);
            }
        }

        // --- NO CHANGES to CreateBulkAsync ---
        public async Task<List<TutorEducationDto>> CreateBulkAsync(List<TutorEducationCreateRequest> requests)
        {
            try
            {
                var results = new List<TutorEducationDto>();
                foreach (var request in requests)
                {
                    var result = await CreateAsync(request);
                    results.Add(result);
                }
                return results;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to create bulk tutor educations: {ex.Message}", ex);
            }
        }

        // --- UPDATED to delete cloud file ---
        public async Task DeleteAsync(int id)
        {
            // Ensure file is deleted from cloud
            var entity = await _repository.GetByIdFullAsync(id);
            if (entity != null)
            {
                await _repository.RemoveByIdAsync(id);
                if (!string.IsNullOrWhiteSpace(entity.CertificatePublicId))
                {
                    // Fire-and-forget deletion
                    _ = _cloudMedia.DeleteByPublicIdAsync(entity.CertificatePublicId, MediaType.Image);
                }
            }
        }

        // --- UPDATED to use the fixed DeleteAsync ---
        public async Task DeleteByTutorIdAsync(int tutorId)
        {
            var entities = await _repository.GetByTutorIdAsync(tutorId);
            foreach (var entity in entities)
            {
                await this.DeleteAsync(entity.Id); // This now correctly handles file cleanup
            }
        }

        // --- ADDED the new ReconcileAsync method ---
        public async Task ReconcileAsync(int tutorId, List<UpdateTutorEducationRequest> incomingEducations)
        {
            // 1. Get current state from DB
            var currentEducations = await _repository.GetByTutorIdAsync(tutorId);
            var currentIds = currentEducations.Select(e => e.Id).ToHashSet();
            var incomingIds = incomingEducations.Select(e => e.Id).ToHashSet();

            // 2. (DELETE) Find items in DB that are NOT in the incoming list
            var idsToDelete = currentIds.Except(incomingIds).ToList();
            foreach (var id in idsToDelete)
            {
                // Use our fixed DeleteAsync to ensure cloud file is also deleted
                await this.DeleteAsync(id);
            }

            // 3. (UPDATE) Find items where Id > 0
            var requestsToUpdate = incomingEducations
                .Where(e => e.Id > 0 && currentIds.Contains(e.Id))
                .ToList();
            foreach (var updateRequest in requestsToUpdate)
            {
                // Use our new UpdateAsync to handle file logic
                await this.UpdateAsync(updateRequest);
            }

            // 4. (CREATE) Find items where Id == 0
            var requestsToCreate = incomingEducations
                .Where(e => e.Id == 0)
                .ToList();
            if (requestsToCreate.Any())
            {
                // Map from UpdateTutorEducationRequest -> TutorEducationCreateRequest
                var createRequests = _mapper.Map<List<TutorEducationCreateRequest>>(requestsToCreate);
                foreach (var createRequest in createRequests)
                {
                    createRequest.TutorId = tutorId; // Set the parent ID
                }
                // Use our existing bulk create method
                await this.CreateBulkAsync(createRequests);
            }
        }
    }
}