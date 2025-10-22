using AutoMapper;
using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Requests;
using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Interfaces;
using EduMatch.DataAccessLayer.Repositories;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Services
{
	public class TutorSubjectService : ITutorSubjectService
	{
		private readonly ITutorSubjectRepository _repository;
		private readonly IMapper _mapper;
		private readonly ITutorProfileRepository _tutorProfileRepository;
		private	readonly ISubjectRepository _subjectRepository;
		private readonly ILevelRepository _levelRepository;

		public TutorSubjectService(
			ITutorSubjectRepository repository,
			IMapper mapper,
			ITutorProfileRepository tutorProfileRepository,
			ISubjectRepository subjectRepository,
			ILevelRepository levelRepository)
		{
			_repository = repository;
			_mapper = mapper;
			_tutorProfileRepository = tutorProfileRepository;
			_subjectRepository = subjectRepository;
			_levelRepository = levelRepository;

		}

		public async Task<TutorSubjectDto?> GetByIdFullAsync(int id)
		{
			var entity = await _repository.GetByIdFullAsync(id);
			return entity != null ? _mapper.Map<TutorSubjectDto>(entity) : null;
		}

		public async Task<TutorSubjectDto?> GetByTutorIdFullAsync(int tutorId)
		{
			var entity = await _repository.GetByTutorIdFullAsync(tutorId);
			return entity != null ? _mapper.Map<TutorSubjectDto>(entity) : null;
		}

		public async Task<IReadOnlyList<TutorSubjectDto>> GetByTutorIdAsync(int tutorId)
		{
			var entities = await _repository.GetByTutorIdAsync(tutorId);
			return _mapper.Map<IReadOnlyList<TutorSubjectDto>>(entities);
		}

		public async Task<IReadOnlyList<TutorSubjectDto>> GetBySubjectIdAsync(int subjectId)
		{
			var entities = await _repository.GetBySubjectIdAsync(subjectId);
			return _mapper.Map<IReadOnlyList<TutorSubjectDto>>(entities);
		}

		public async Task<IReadOnlyList<TutorSubjectDto>> GetByLevelIdAsync(int levelId)
		{
			var entities = await _repository.GetByLevelIdAsync(levelId);
			return _mapper.Map<IReadOnlyList<TutorSubjectDto>>(entities);
		}

		public async Task<IReadOnlyList<TutorSubjectDto>> GetByHourlyRateRangeAsync(decimal minRate, decimal maxRate)
		{
			var entities = await _repository.GetByHourlyRateRangeAsync(minRate, maxRate);
			return _mapper.Map<IReadOnlyList<TutorSubjectDto>>(entities);
		}

		public async Task<IReadOnlyList<TutorSubjectDto>> GetTutorsBySubjectAndLevelAsync(int subjectId, int levelId)
		{
			var entities = await _repository.GetTutorsBySubjectAndLevelAsync(subjectId, levelId);
			return _mapper.Map<IReadOnlyList<TutorSubjectDto>>(entities);
		}

		public async Task<IReadOnlyList<TutorSubjectDto>> GetAllFullAsync()
		{
			var entities = await _repository.GetAllFullAsync();
			return _mapper.Map<IReadOnlyList<TutorSubjectDto>>(entities);
		}

		public async Task<TutorSubjectDto> CreateAsync(TutorSubjectCreateRequest request)
		{
			try
			{
				// Validate request
				var validationContext = new ValidationContext(request);
				var validationResults = new List<ValidationResult>();
				if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
				{
					throw new ArgumentException($"Validation failed: {string.Join(", ", validationResults.Select(r => r.ErrorMessage))}");
				}

				var tutor = await _tutorProfileRepository.GetByIdFullAsync(request.TutorId);
				if (tutor is null)
					throw new ArgumentException($"Tutor with ID {request.TutorId} not found.");

				var subject = await _subjectRepository.GetByIdAsync(request.SubjectId);
				if (subject is null)
					throw new ArgumentException($"subject with ID {request.SubjectId} not found.");

				var level = await _levelRepository.GetByIdAsync(request.LevelId);
				if (level is null)
					throw new ArgumentException($"level with ID {request.LevelId} not found.");

				var entity = _mapper.Map<TutorSubject>(request);
				await _repository.AddAsync(entity);
				return _mapper.Map<TutorSubjectDto>(entity);
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException($"Failed to create tutor subject: {ex.Message}", ex);
			}
		}

		//public async Task<TutorSubjectDto> UpdateAsync(TutorSubjectUpdateRequest request)
		//{
		//	try
		//	{
		//		// Validate request
		//		var validationContext = new ValidationContext(request);
		//		var validationResults = new List<ValidationResult>();
		//		if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
		//		{
		//			throw new ArgumentException($"Validation failed: {string.Join(", ", validationResults.Select(r => r.ErrorMessage))}");
		//		}

		//		// Check if entity exists
		//		var existingEntity = await _repository.GetByIdFullAsync(request.Id);
		//		if (existingEntity == null)
		//		{
		//			throw new ArgumentException($"Tutor subject with ID {request.Id} not found");
		//		}

		//		var entity = _mapper.Map<TutorSubject>(request);
		//		await _repository.UpdateAsync(entity);
		//		return _mapper.Map<TutorSubjectDto>(entity);
		//	}
		//	catch (Exception ex)
		//	{
		//		throw new InvalidOperationException($"Failed to update tutor subject: {ex.Message}", ex);
		//	}
		//}

		public async Task<List<TutorSubjectDto>> CreateBulkAsync(List<TutorSubjectCreateRequest> requests)
		{
			try
			{
				var results = new List<TutorSubjectDto>();
				foreach (var request in requests)
				{
					var result = await CreateAsync(request);
					results.Add(result);
				}
				return results;
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException($"Failed to create bulk tutor subjects: {ex.Message}", ex);
			}
		}

      


        public async Task DeleteAsync(int id)
		{
			await _repository.RemoveByIdAsync(id);
		}

		public async Task DeleteByTutorIdAsync(int tutorId)
		{
			await _repository.RemoveByTutorIdAsync(tutorId);
		}


        public async Task ReconcileAsync(int tutorId, List<UpdateTutorSubjectRequest> incomingSubjects)
        {
            // 1. Get current state from DB
            // Your repo uses AsNoTracking(), which is fine
            var currentSubjects = await _repository.GetByTutorIdAsync(tutorId);
            var currentIds = currentSubjects.Select(s => s.Id).ToHashSet();
            var incomingIds = incomingSubjects.Select(s => s.Id).ToHashSet();

            // 2. (DELETE) Loop and call RemoveByIdAsync
            var idsToDelete = currentIds.Except(incomingIds);
            foreach (var id in idsToDelete)
            {
                await _repository.RemoveByIdAsync(id);
            }

            // 3. (UPDATE) Loop and call UpdateAsync
            var requestsToUpdate = incomingSubjects
                .Where(s => s.Id > 0 && currentIds.Contains(s.Id))
                .ToList();
            foreach (var req in requestsToUpdate)
            {
                // Find the matching entity
                var entity = currentSubjects.First(s => s.Id == req.Id);

                // Map the changes from the DTO onto the entity
                _mapper.Map(req, entity);

                // Call your repository's UpdateAsync
                await _repository.UpdateAsync(entity);
            }

            // 4. (CREATE) Loop and call AddAsync
            var requestsToCreate = incomingSubjects
                .Where(s => s.Id == 0)
                .ToList();
            foreach (var req in requestsToCreate)
            {
                var newEntity = _mapper.Map<TutorSubject>(req);
                newEntity.TutorId = tutorId; // Set parent ID

                // Call your repository's AddAsync
                await _repository.AddAsync(newEntity);
            }
        }

        // --- I RECOMMEND FIXING YOUR UpdateAsync LIKE THIS ---
        // Your old version was re-mapping the entire object,
        // which could be dangerous. This is safer.
        public async Task<TutorSubjectDto> UpdateAsync(TutorSubjectUpdateRequest request)
        {
            try
            {
                // Check if entity exists
                var existingEntity = await _repository.GetByIdFullAsync(request.Id);
                if (existingEntity == null)
                {
                    throw new ArgumentException($"Tutor subject with ID {request.Id} not found");
                }

                // Map the changes onto the existing entity
                _mapper.Map(request, existingEntity);

                await _repository.UpdateAsync(existingEntity);
                return _mapper.Map<TutorSubjectDto>(existingEntity);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to update tutor subject: {ex.Message}", ex);
            }
        }
    }
}
