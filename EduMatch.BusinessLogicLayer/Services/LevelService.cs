using AutoMapper;
using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Requests.Level;
using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace EduMatch.BusinessLogicLayer.Services
{
	public class LevelService : ILevelService
	{
		private readonly ILevelRepository _levelRepository;
		private readonly IMapper _mapper;

		public LevelService(ILevelRepository repository, IMapper mapper)
		{
			_levelRepository = repository;
			_mapper = mapper;
		}

		/// <summary>
		/// Lấy Level theo ID
		/// </summary>
		public async Task<LevelDto?> GetByIdAsync(int id)
		{
			if (id <= 0)
				throw new ArgumentException("ID must be greater than 0");
			var entity = await _levelRepository.GetByIdAsync(id);
			return entity != null ? _mapper.Map<LevelDto>(entity) : null;
		}

		/// <summary>
		/// Lấy tất cả Level
		/// </summary>
		public async Task<IReadOnlyList<LevelDto>> GetAllAsync()
		{
			var entities = await _levelRepository.GetAllAsync();
			return _mapper.Map<IReadOnlyList<LevelDto>>(entities);
		}

		/// <summary>
		/// Tìm Level theo tên
		/// </summary>
		public async Task<IReadOnlyList<LevelDto>> GetByNameAsync(string name)
		{
			if (string.IsNullOrWhiteSpace(name))
				throw new ArgumentException("Name is required");
			var entities = await _levelRepository.GetByNameAsync(name);
			return _mapper.Map<IReadOnlyList<LevelDto>>(entities);
		}

		/// <summary>
		/// Tạo Level mới
		/// </summary>
		public async Task<LevelDto> CreateAsync(LevelCreateRequest request)
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

				var entity = new Level
				{
					Name = request.Name
				};
				await _levelRepository.AddAsync(entity);
				return _mapper.Map<LevelDto>(entity);
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException($"Failed to create level: {ex.Message}", ex);
			}
		}

		/// <summary>
		/// Cập nhật Level
		/// </summary>
		public async Task<LevelDto> UpdateAsync(LevelUpdateRequest request)
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

				// Check if entity exists
				var existingEntity = await _levelRepository.GetByIdAsync(request.Id);
				if (existingEntity == null)
				{
					throw new ArgumentException($"Level with ID {request.Id} not found");
				}

				// Update only provided fields
				existingEntity.Name = request.Name;

				await _levelRepository.UpdateAsync(existingEntity);
				return _mapper.Map<LevelDto>(existingEntity);
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException($"Failed to update level: {ex.Message}", ex);
			}
		}

		/// <summary>
		/// Xóa Level theo ID
		/// </summary>
		public async Task DeleteAsync(int id)
		{
			if (id <= 0)
				throw new ArgumentException("ID must be greater than 0");
			await _levelRepository.RemoveByIdAsync(id);
		}
	}
}
