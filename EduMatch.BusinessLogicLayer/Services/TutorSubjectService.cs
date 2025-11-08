using AutoMapper;
using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Requests.TutorSubject;
using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Interfaces;
using EduMatch.DataAccessLayer.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Services
{
	public class TutorSubjectService : ITutorSubjectService
	{
		private readonly ITutorSubjectRepository _tutorSubjectRepository;
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
			_tutorSubjectRepository = repository;
			_mapper = mapper;
			_tutorProfileRepository = tutorProfileRepository;
			_subjectRepository = subjectRepository;
			_levelRepository = levelRepository;

		}

		/// <summary>
		/// Lấy TutorSubject theo ID với đầy đủ thông tin
		/// </summary>
		public async Task<TutorSubjectDto?> GetByIdFullAsync(int id)
		{
			if (id <= 0)
				throw new ArgumentException("ID must be greater than 0");
			var entity = await _tutorSubjectRepository.GetByIdFullAsync(id);
			return entity != null ? _mapper.Map<TutorSubjectDto>(entity) : null;
		}

		/// <summary>
		/// Lấy TutorSubject theo TutorId với đầy đủ thông tin
		/// </summary>
		public async Task<TutorSubjectDto?> GetByTutorIdFullAsync(int tutorId)
		{
			if (tutorId <= 0)
				throw new ArgumentException("TutorId must be greater than 0");
			var entity = await _tutorSubjectRepository.GetByTutorIdFullAsync(tutorId);
			return entity != null ? _mapper.Map<TutorSubjectDto>(entity) : null;
		}

		/// <summary>
		/// Lấy danh sách TutorSubject theo TutorId
		/// </summary>
		public async Task<IReadOnlyList<TutorSubjectDto>> GetByTutorIdAsync(int tutorId)
		{
			if (tutorId <= 0)
				throw new ArgumentException("TutorId must be greater than 0");
			var entities = await _tutorSubjectRepository.GetByTutorIdAsync(tutorId);
			return _mapper.Map<IReadOnlyList<TutorSubjectDto>>(entities);
		}

		/// <summary>
		/// Lấy danh sách TutorSubject theo SubjectId
		/// </summary>
		public async Task<IReadOnlyList<TutorSubjectDto>> GetBySubjectIdAsync(int subjectId)
		{
			if (subjectId <= 0)
				throw new ArgumentException("SubjectId must be greater than 0");
			var entities = await _tutorSubjectRepository.GetBySubjectIdAsync(subjectId);
			return _mapper.Map<IReadOnlyList<TutorSubjectDto>>(entities);
		}

		/// <summary>
		/// Lấy danh sách TutorSubject theo LevelId
		/// </summary>
		public async Task<IReadOnlyList<TutorSubjectDto>> GetByLevelIdAsync(int levelId)
		{
			if (levelId <= 0)
				throw new ArgumentException("LevelId must be greater than 0");
			var entities = await _tutorSubjectRepository.GetByLevelIdAsync(levelId);
			return _mapper.Map<IReadOnlyList<TutorSubjectDto>>(entities);
		}

		/// <summary>
		/// Lấy danh sách TutorSubject theo khoảng giá giờ
		/// </summary>
		public async Task<IReadOnlyList<TutorSubjectDto>> GetByHourlyRateRangeAsync(decimal minRate, decimal maxRate)
		{
			var entities = await _tutorSubjectRepository.GetByHourlyRateRangeAsync(minRate, maxRate);
			return _mapper.Map<IReadOnlyList<TutorSubjectDto>>(entities);
		}

		/// <summary>
		/// Lấy danh sách TutorSubject theo SubjectId và LevelId
		/// </summary>
		public async Task<IReadOnlyList<TutorSubjectDto>> GetTutorsBySubjectAndLevelAsync(int subjectId, int levelId)
		{
			var entities = await _tutorSubjectRepository.GetTutorsBySubjectAndLevelAsync(subjectId, levelId);
			return _mapper.Map<IReadOnlyList<TutorSubjectDto>>(entities);
		}

		/// <summary>
		/// Lấy tất cả TutorSubject với đầy đủ thông tin
		/// </summary>
		public async Task<IReadOnlyList<TutorSubjectDto>> GetAllFullAsync()
		{
			var entities = await _tutorSubjectRepository.GetAllFullAsync();
			return _mapper.Map<IReadOnlyList<TutorSubjectDto>>(entities);
		}

		/// <summary>
		/// Tạo TutorSubject mới
		/// </summary>
	public async Task<TutorSubjectDto> CreateAsync(TutorSubjectCreateRequest request)
	{
		try
		{
			var tutor = await _tutorProfileRepository.GetByIdFullAsync(request.TutorId);
				if (tutor is null)
					throw new ArgumentException($"Tutor with ID {request.TutorId} not found.");

				var subject = await _subjectRepository.GetByIdAsync(request.SubjectId);
				if (subject is null)
					throw new ArgumentException($"subject with ID {request.SubjectId} not found.");

				var level = await _levelRepository.GetByIdAsync(request.LevelId);
				if (level is null)
					throw new ArgumentException($"level with ID {request.LevelId} not found.");

				var entity = new TutorSubject
				{
					TutorId = request.TutorId,
					SubjectId = request.SubjectId,
					LevelId = request.LevelId,
					HourlyRate = request.HourlyRate
				};
				await _tutorSubjectRepository.AddAsync(entity);
				return _mapper.Map<TutorSubjectDto>(entity);
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException($"Failed to create tutor subject: {ex.Message}", ex);
			}
		}

        /// <summary>
        /// Cập nhật TutorSubject
        /// </summary>
        public async Task<TutorSubjectDto> UpdateAsync(TutorSubjectUpdateRequest request)
        {
            try
            {
                // Check if entity exists
				var existingEntity = await _tutorSubjectRepository.GetByIdFullAsync(request.Id);
                if (existingEntity == null)
                {
                    throw new ArgumentException($"Tutor subject with ID {request.Id} not found");
                }

                // Update only provided fields
                existingEntity.TutorId = request.TutorId;
                existingEntity.SubjectId = request.SubjectId;
                if (request.LevelId.HasValue) 
                    existingEntity.LevelId = request.LevelId.Value;
                if (request.HourlyRate.HasValue) 
                    existingEntity.HourlyRate = request.HourlyRate.Value;

			await _tutorSubjectRepository.UpdateAsync(existingEntity);
                return _mapper.Map<TutorSubjectDto>(existingEntity);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to update tutor subject: {ex.Message}", ex);
            }
        }

		/// <summary>
		/// Tạo nhiều TutorSubject
		/// </summary>
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

		/// <summary>
		/// Xóa TutorSubject theo ID
		/// </summary>
		public async Task DeleteAsync(int id)
		{
			if (id <= 0)
				throw new ArgumentException("ID must be greater than 0");
			await _tutorSubjectRepository.RemoveByIdAsync(id);
		}

		/// <summary>
		/// Xóa tất cả TutorSubject theo TutorId
		/// </summary>
		public async Task DeleteByTutorIdAsync(int tutorId)
		{
			if (tutorId <= 0)
				throw new ArgumentException("TutorId must be greater than 0");
			await _tutorSubjectRepository.RemoveByTutorIdAsync(tutorId);
		}
	}
}
