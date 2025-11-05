using AutoMapper;
using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Requests.Subject;
using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace EduMatch.BusinessLogicLayer.Services
{
	public class SubjectService : ISubjectService
	{
		private readonly ISubjectRepository _subjectRepository;
		private readonly IMapper _mapper;

		public SubjectService(ISubjectRepository repository, IMapper mapper)
		{
			_subjectRepository = repository;
			_mapper = mapper;
		}

		/// <summary>
		/// Lấy Subject theo ID
		/// </summary>
		public async Task<SubjectDto?> GetByIdAsync(int id)
		{
			var entity = await _subjectRepository.GetByIdAsync(id);
			return entity != null ? _mapper.Map<SubjectDto>(entity) : null;
		}

		/// <summary>
		/// Lấy tất cả Subject
		/// </summary>
		public async Task<IReadOnlyList<SubjectDto>> GetAllAsync()
		{
			var entities = await _subjectRepository.GetAllAsync();
			return _mapper.Map<IReadOnlyList<SubjectDto>>(entities);
		}

		/// <summary>
		/// Tìm Subject theo tên
		/// </summary>
		public async Task<IReadOnlyList<SubjectDto>> GetByNameAsync(string name)
		{
			var entities = await _subjectRepository.GetByNameAsync(name);
			return _mapper.Map<IReadOnlyList<SubjectDto>>(entities);
		}

		/// <summary>
		/// Tạo Subject mới
		/// </summary>
	public async Task<SubjectDto> CreateAsync(SubjectCreateRequest request)
	{
		try
		{
			var entity = new Subject
				{
					SubjectName = request.SubjectName
				};
				await _subjectRepository.AddAsync(entity);
				return _mapper.Map<SubjectDto>(entity);
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException($"Failed to create subject: {ex.Message}", ex);
			}
		}

		/// <summary>
		/// Cập nhật Subject
		/// </summary>
	public async Task<SubjectDto> UpdateAsync(SubjectUpdateRequest request)
	{
		try
		{
			// Check if entity exists
				var existingEntity = await _subjectRepository.GetByIdAsync(request.Id);
				if (existingEntity == null)
				{
					throw new ArgumentException($"Subject with ID {request.Id} not found");
				}

				// Update only provided fields
				existingEntity.SubjectName = request.SubjectName;

				await _subjectRepository.UpdateAsync(existingEntity);
				return _mapper.Map<SubjectDto>(existingEntity);
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException($"Failed to update subject: {ex.Message}", ex);
			}
		}

		/// <summary>
		/// Xóa Subject theo ID
		/// </summary>
		public async Task DeleteAsync(int id)
		{
			await _subjectRepository.RemoveByIdAsync(id);
		}
	}
}
