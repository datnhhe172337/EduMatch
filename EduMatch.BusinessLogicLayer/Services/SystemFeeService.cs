using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Requests.SystemFee;
using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Interfaces;

namespace EduMatch.BusinessLogicLayer.Services
{
    public class SystemFeeService : ISystemFeeService
    {
        private readonly ISystemFeeRepository _systemFeeRepository;
        private readonly IMapper _mapper;
        public SystemFeeService(ISystemFeeRepository systemFeeRepository, IMapper mapper)
        {
            _systemFeeRepository = systemFeeRepository;
            _mapper = mapper;
        }

        /// <summary>
        /// Lấy danh sách SystemFee với phân trang
        /// </summary>
        public async Task<List<SystemFeeDto>> GetAllAsync(int page = 1, int pageSize = 10)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;
            var entities = await _systemFeeRepository.GetAllAsync(page, pageSize);
            return  _mapper.Map<List<SystemFeeDto>>(entities);
        }

        /// <summary>
        /// Lấy tất cả SystemFee (không phân trang)
        /// </summary>
        public async Task<List<SystemFeeDto>> GetAllNoPagingAsync()
        {
            var entities = await _systemFeeRepository.GetAllNoPagingAsync();
            return _mapper.Map<List<SystemFeeDto>>(entities);
        }

        /// <summary>
        /// Đếm tổng số SystemFee
        /// </summary>
        public Task<int> CountAsync() => _systemFeeRepository.CountAsync();

        /// <summary>
        /// Lấy SystemFee theo ID
        /// </summary>
        public async Task<SystemFeeDto?> GetByIdAsync(int id)
        {
            var entity = await _systemFeeRepository.GetByIdAsync(id);
            return entity == null ? null : _mapper.Map<SystemFeeDto>(entity);
        }

        /// <summary>
        /// Tạo SystemFee mới
        /// </summary>
        public async Task<SystemFeeDto> CreateAsync(SystemFeeCreateRequest request)
        {
            var now = DateTime.UtcNow;
            var entity = new SystemFee
            {
                Name = request.Name,
                Percentage = request.Percentage,
                FixedAmount = request.FixedAmount,
                EffectiveFrom = now,
                EffectiveTo = null,
                IsActive = request.IsActive ?? true,
                CreatedAt = now,
                UpdatedAt = null
            };

            await _systemFeeRepository.CreateAsync(entity);
            return _mapper.Map<SystemFeeDto>(entity);
        }

        /// <summary>
        /// Cập nhật SystemFee
        /// </summary>
        public async Task<SystemFeeDto> UpdateAsync(SystemFeeUpdateRequest request)
        {
            var entity = await _systemFeeRepository.GetByIdAsync(request.Id) ?? throw new Exception("SystemFee không tồn tại");

            if (!string.IsNullOrWhiteSpace(request.Name)) entity.Name = request.Name;
            if (request.Percentage.HasValue) entity.Percentage = request.Percentage.Value;
            if (request.FixedAmount.HasValue) entity.FixedAmount = request.FixedAmount.Value;
            if (request.IsActive.HasValue) entity.IsActive = request.IsActive.Value;

            entity.UpdatedAt = DateTime.UtcNow;

            await _systemFeeRepository.UpdateAsync(entity);
            return _mapper.Map<SystemFeeDto>(entity);
        }

        /// <summary>
        /// Xóa SystemFee theo ID
        /// </summary>
        public async Task DeleteAsync(int id)
        {
            await _systemFeeRepository.DeleteAsync(id);
        }

        /// <summary>
        /// Lấy SystemFee đang hoạt động (IsActive = true), lấy giá trị đầu tiên
        /// </summary>
        public async Task<SystemFeeDto?> GetActiveSystemFeeAsync()
        {
            var entity = await _systemFeeRepository.GetActiveSystemFeeAsync();
            return entity == null ? null : _mapper.Map<SystemFeeDto>(entity);
        }
    }
}
