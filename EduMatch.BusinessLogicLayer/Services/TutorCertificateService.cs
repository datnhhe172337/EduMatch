using AutoMapper;
using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Requests;
using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Enum;
using EduMatch.DataAccessLayer.Interfaces;


namespace EduMatch.BusinessLogicLayer.Services
{
    public class TutorCertificateService : ITutorCertificateService
    {
        private readonly ITutorCertificateRepository _repository;
        private readonly IMapper _mapper;
        private readonly ICloudMediaService _cloudMedia;
        private readonly CurrentUserService _currentUserService;
        private readonly ITutorProfileRepository _tutorProfileRepository;
        private readonly ICertificateTypeRepository _certificateTypeRepository;

        public TutorCertificateService(
            ITutorCertificateRepository repository,
            IMapper mapper,
            ICloudMediaService cloudMedia,
            CurrentUserService currentUserService,
            ITutorProfileRepository tutorProfileRepository,
            ICertificateTypeRepository certificateTypeRepository)
        {
            _repository = repository;
            _mapper = mapper;
            _cloudMedia = cloudMedia;
            _currentUserService = currentUserService;
            _tutorProfileRepository = tutorProfileRepository;
            _certificateTypeRepository = certificateTypeRepository;
        }

        // --- NO CHANGES to any of your Get... methods ---
        public async Task<TutorCertificateDto?> GetByIdFullAsync(int id)
        {
            var entity = await _repository.GetByIdFullAsync(id);
            return entity != null ? _mapper.Map<TutorCertificateDto>(entity) : null;
        }

        public async Task<TutorCertificateDto?> GetByTutorIdFullAsync(int tutorId)
        {
            var entity = await _repository.GetByTutorIdFullAsync(tutorId);
            return entity != null ? _mapper.Map<TutorCertificateDto>(entity) : null;
        }

        public async Task<IReadOnlyList<TutorCertificateDto>> GetByTutorIdAsync(int tutorId)
        {
            var entities = await _repository.GetByTutorIdAsync(tutorId);
            return _mapper.Map<IReadOnlyList<TutorCertificateDto>>(entities);
        }
        // ... all your other Get... methods ...
        public async Task<IReadOnlyList<TutorCertificateDto>> GetByCertificateTypeAsync(int certificateTypeId)
        {
            var entities = await _repository.GetByCertificateTypeAsync(certificateTypeId);
            return _mapper.Map<IReadOnlyList<TutorCertificateDto>>(entities);
        }

        public async Task<IReadOnlyList<TutorCertificateDto>> GetByVerifiedStatusAsync(VerifyStatus verified)
        {
            var entities = await _repository.GetByVerifiedStatusAsync(verified);
            return _mapper.Map<IReadOnlyList<TutorCertificateDto>>(entities);
        }

        public async Task<IReadOnlyList<TutorCertificateDto>> GetExpiredCertificatesAsync()
        {
            var entities = await _repository.GetExpiredCertificatesAsync();
            return _mapper.Map<IReadOnlyList<TutorCertificateDto>>(entities);
        }

        public async Task<IReadOnlyList<TutorCertificateDto>> GetExpiringCertificatesAsync(DateTime beforeDate)
        {
            var entities = await _repository.GetExpiringCertificatesAsync(beforeDate);
            return _mapper.Map<IReadOnlyList<TutorCertificateDto>>(entities);
        }

        public async Task<IReadOnlyList<TutorCertificateDto>> GetAllFullAsync()
        {
            var entities = await _repository.GetAllFullAsync();
            return _mapper.Map<IReadOnlyList<TutorCertificateDto>>(entities);
        }


        // --- UPDATED to use AutoMapper ---
        public async Task<TutorCertificateDto> CreateAsync(TutorCertificateCreateRequest request)
        {
            try
            {
                // ... (Your validation logic is perfect, no changes) ...
                var tutor = await _tutorProfileRepository.GetByIdFullAsync(request.TutorId);
                if (tutor is null)
                    throw new ArgumentException($"Tutor with ID {request.TutorId} not found.");
                var certificateType = await _certificateTypeRepository.GetByIdAsync(request.CertificateTypeId);
                if (certificateType is null)
                    throw new ArgumentException($"CertificateType with ID {request.CertificateTypeId} not found.");
                if (_currentUserService.Email is null)
                    throw new ArgumentException("Current user email not found.");


                string? certUrl = null;
                string? certPublicId = null;
                var hasFile = request.Certificate != null && request.Certificate.Length > 0 && !string.IsNullOrWhiteSpace(request.Certificate.FileName);
                if (hasFile)
                {
                    // ... (Your file upload logic is perfect, no changes) ...
                    using var stream = request.Certificate!.OpenReadStream();
                    var uploadRequest = new UploadToCloudRequest(
                        Content: stream,
                        FileName: request.Certificate!.FileName,
                        ContentType: request.Certificate!.ContentType ?? "application/octet-stream",
                        LengthBytes: request.Certificate!.Length,
                        OwnerEmail: _currentUserService.Email!,
                        MediaType: MediaType.Image
                    );
                    var uploadResult = await _cloudMedia.UploadAsync(uploadRequest);
                    if (!uploadResult.Ok || string.IsNullOrEmpty(uploadResult.SecureUrl))
                        throw new InvalidOperationException($"Failed to upload file: {uploadResult.ErrorMessage}");
                    certUrl = uploadResult.SecureUrl;
                    certPublicId = uploadResult.PublicId;
                }

                // --- UPDATED ---
                // MAP  -> ENTITY (using AutoMapper)
                var entity = _mapper.Map<TutorCertificate>(request);

                // Set non-mapped properties
                entity.CertificateUrl = certUrl;
                entity.CertificatePublicId = certPublicId;
                entity.CreatedAt = DateTime.UtcNow;
                entity.Verified = VerifyStatus.Pending;
                entity.RejectReason = null;

                await _repository.AddAsync(entity);
                return _mapper.Map<TutorCertificateDto>(entity);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to create tutor certificate: {ex.Message}", ex);
            }
        }

        // --- YOUR EXISTING ADMIN-LEVEL UPDATE (NO CHANGES) ---
        public async Task<TutorCertificateDto> UpdateAsync(TutorCertificateUpdateRequest request)
        {
            try
            {
                // ... (Your existing validation, file upload, and logic) ...
                // This logic is correct for an ADMIN.
                var existingEntity = await _repository.GetByIdFullAsync(request.Id);
                if (existingEntity == null)
                {
                    throw new ArgumentException($"Tutor certificate with ID {request.Id} not found");
                }

                // ... all your logic for file upload, verify, reject reason ...
                var oldPublicId = existingEntity.CertificatePublicId;
                var hasNewFile = request.Certificate != null && request.Certificate.Length > 0 && !string.IsNullOrWhiteSpace(request.Certificate.FileName);
                if (hasNewFile)
                {
                    using var stream = request.Certificate!.OpenReadStream();
                    var uploadRequest = new UploadToCloudRequest(
                        Content: stream,
                        FileName: request.Certificate!.FileName,
                        ContentType: request.Certificate!.ContentType ?? "application/octet-stream",
                        LengthBytes: request.Certificate!.Length,
                        OwnerEmail: _currentUserService.Email!,
                        MediaType: MediaType.Image
                    );
                    var uploadResult = await _cloudMedia.UploadAsync(uploadRequest);
                    if (!uploadResult.Ok || string.IsNullOrEmpty(uploadResult.SecureUrl))
                        throw new InvalidOperationException($"Failed to upload file: {uploadResult.ErrorMessage}");
                    existingEntity.CertificateUrl = uploadResult.SecureUrl;
                    existingEntity.CertificatePublicId = uploadResult.PublicId;
                }

                existingEntity.TutorId = request.TutorId;
                existingEntity.CertificateTypeId = request.CertificateTypeId;
                if (request.Verified.HasValue) existingEntity.Verified = request.Verified.Value;
                if (request.IssueDate.HasValue) existingEntity.IssueDate = request.IssueDate.Value;
                if (request.ExpiryDate.HasValue) existingEntity.ExpiryDate = request.ExpiryDate.Value;
                if (request.Verified == VerifyStatus.Rejected)
                {
                    if (string.IsNullOrWhiteSpace(request.RejectReason))
                        throw new ArgumentException("Reject reason is required when verification status is Rejected.");
                    existingEntity.RejectReason = request.RejectReason!.Trim();
                }
                else
                {
                    existingEntity.RejectReason = null;
                }

                await _repository.UpdateAsync(existingEntity);

                if (hasNewFile && !string.IsNullOrWhiteSpace(oldPublicId))
                {
                    _ = _cloudMedia.DeleteByPublicIdAsync(oldPublicId, MediaType.Image);
                }

                return _mapper.Map<TutorCertificateDto>(existingEntity);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to update tutor certificate: {ex.Message}", ex);
            }
        }

        // --- NEW TUTOR-LEVEL UPDATE (FOR RECONCILIATION) ---
        public async Task<TutorCertificateDto> UpdateAsync(UpdateTutorCertificateRequest request)
        {
            try
            {
                var existingEntity = await _repository.GetByIdFullAsync(request.Id);
                if (existingEntity == null)
                    throw new ArgumentException($"Certificate with ID {request.Id} not found");

                var oldPublicId = existingEntity.CertificatePublicId;
                var hasNewFile = request.Certificate != null && request.Certificate.Length > 0;

                if (hasNewFile)
                {
                    using var stream = request.Certificate!.OpenReadStream();
                    var uploadResult = await _cloudMedia.UploadAsync(new UploadToCloudRequest(
                        Content: stream,
                        FileName: request.Certificate.FileName,
                        ContentType: request.Certificate.ContentType ?? "application/octet-stream",
                        LengthBytes: request.Certificate.Length,
                        OwnerEmail: _currentUserService.Email!,
                        MediaType: MediaType.Image
                    ));
                    if (!uploadResult.Ok) throw new InvalidOperationException("File upload failed.");

                    existingEntity.CertificateUrl = uploadResult.SecureUrl;
                    existingEntity.CertificatePublicId = uploadResult.PublicId;
                }
                else if (request.RemoveCertificate && !string.IsNullOrWhiteSpace(oldPublicId))
                {
                    existingEntity.CertificateUrl = null;
                    existingEntity.CertificatePublicId = null;
                }

                // Map other properties
                existingEntity.CertificateTypeId = request.CertificateTypeId;
                existingEntity.IssueDate = request.IssueDate;
                existingEntity.ExpiryDate = request.ExpiryDate;

                // When a tutor updates, always reset to Pending
                existingEntity.Verified = VerifyStatus.Pending;
                existingEntity.RejectReason = null;

                await _repository.UpdateAsync(existingEntity);

                if ((hasNewFile || request.RemoveCertificate) && !string.IsNullOrWhiteSpace(oldPublicId))
                {
                    _ = _cloudMedia.DeleteByPublicIdAsync(oldPublicId, MediaType.Image);
                }

                return _mapper.Map<TutorCertificateDto>(existingEntity);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to update tutor certificate: {ex.Message}", ex);
            }
        }

        // --- NO CHANGES to CreateBulkAsync ---
        public async Task<List<TutorCertificateDto>> CreateBulkAsync(List<TutorCertificateCreateRequest> requests)
        {
            try
            {
                var results = new List<TutorCertificateDto>();
                foreach (var request in requests)
                {
                    var result = await CreateAsync(request);
                    results.Add(result);
                }
                return results;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to create bulk tutor certificates: {ex.Message}", ex);
            }
        }

        // --- UPDATED to delete cloud file ---
        public async Task DeleteAsync(int id)
        {
            var entity = await _repository.GetByIdFullAsync(id); // Use GetByIdFullAsync
            if (entity != null)
            {
                await _repository.RemoveByIdAsync(id);
                if (!string.IsNullOrWhiteSpace(entity.CertificatePublicId))
                {
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

        // --- NEW RECONCILIATION METHOD ---
        public async Task ReconcileAsync(int tutorId, List<UpdateTutorCertificateRequest> incomingCertificates)
        {
            // 1. Get current state
            var currentCerts = await _repository.GetByTutorIdAsync(tutorId);
            var currentIds = currentCerts.Select(c => c.Id).ToHashSet();
            var incomingIds = incomingCertificates.Select(c => c.Id).ToHashSet();

            // 2. (DELETE)
            var idsToDelete = currentIds.Except(incomingIds);
            foreach (var id in idsToDelete)
            {
                await this.DeleteAsync(id); // Use self.DeleteAsync for file cleanup
            }

            // 3. (UPDATE)
            var requestsToUpdate = incomingCertificates
                .Where(c => c.Id > 0 && currentIds.Contains(c.Id))
                .ToList();
            foreach (var req in requestsToUpdate)
            {
                // Call the new UpdateAsync overload
                await this.UpdateAsync(req);
            }

            // 4. (CREATE)
            var requestsToCreate = incomingCertificates
                .Where(c => c.Id == 0)
                .ToList();
            if (requestsToCreate.Any())
            {
                var createDtos = _mapper.Map<List<TutorCertificateCreateRequest>>(requestsToCreate);
                foreach (var dto in createDtos)
                {
                    dto.TutorId = tutorId;
                }
                await this.CreateBulkAsync(createDtos);
            }
        }
    }
}