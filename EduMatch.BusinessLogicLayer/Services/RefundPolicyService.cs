using AutoMapper;
using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Requests.RefundPolicy;
using EduMatch.BusinessLogicLayer.Services;
using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Services
{
    public class RefundPolicyService : IRefundPolicyService
    {
        private readonly IRefundPolicyRepository _refundPolicyRepository;
        private readonly IMapper _mapper;
        private readonly CurrentUserService _currentUserService;

        public RefundPolicyService(
            IRefundPolicyRepository refundPolicyRepository,
            IMapper mapper,
            CurrentUserService currentUserService)
        {
            _refundPolicyRepository = refundPolicyRepository;
            _mapper = mapper;
            _currentUserService = currentUserService;
        }

        /// <summary>
        /// Lấy tất cả RefundPolicy với lọc theo IsActive
        /// </summary>
        public async Task<List<RefundPolicyDto>> GetAllAsync(bool? isActive = null)
        {
            try
            {
                var entities = await _refundPolicyRepository.GetAllAsync(isActive);
                return _mapper.Map<List<RefundPolicyDto>>(entities);
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi lấy danh sách chính sách hoàn tiền: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Lấy RefundPolicy theo ID
        /// </summary>
        public async Task<RefundPolicyDto?> GetByIdAsync(int id)
        {
            try
            {
                if (id <= 0)
                    throw new Exception("Id phải lớn hơn 0");

                var entity = await _refundPolicyRepository.GetByIdAsync(id);
                return entity == null ? null : _mapper.Map<RefundPolicyDto>(entity);
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi lấy chính sách hoàn tiền: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Tạo RefundPolicy mới
        /// </summary>
        public async Task<RefundPolicyDto> CreateAsync(RefundPolicyCreateRequest request)
        {
            try
            {
                if (request == null)
                    throw new Exception("Yêu cầu không được để trống");

                if (string.IsNullOrWhiteSpace(request.Name))
                    throw new Exception("Tên chính sách hoàn tiền không được để trống");

                if (request.RefundPercentage < 0 || request.RefundPercentage > 100)
                    throw new Exception("Tỷ lệ hoàn tiền phải từ 0 đến 100");

                var currentUserEmail = _currentUserService.Email;
                if (string.IsNullOrWhiteSpace(currentUserEmail))
                    throw new Exception("Không thể xác định người dùng hiện tại");

                var now = DateTime.UtcNow;
                var entity = new RefundPolicy
                {
                    Name = request.Name,
                    Description = request.Description,
                    RefundPercentage = request.RefundPercentage,
                    IsActive = true, // Mặc định là active khi tạo mới
                    CreatedAt = now,
                    CreatedBy = currentUserEmail,
                    UpdatedAt = null,
                    UpdatedBy = null
                };

                await _refundPolicyRepository.CreateAsync(entity);
                return _mapper.Map<RefundPolicyDto>(entity);
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi tạo chính sách hoàn tiền: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Cập nhật RefundPolicy
        /// </summary>
        public async Task<RefundPolicyDto> UpdateAsync(RefundPolicyUpdateRequest request)
        {
            try
            {
                if (request == null)
                    throw new Exception("Yêu cầu không được để trống");

                if (request.Id <= 0)
                    throw new Exception("Id phải lớn hơn 0");

                var entity = await _refundPolicyRepository.GetByIdAsync(request.Id);
                if (entity == null)
                    throw new Exception("Chính sách hoàn tiền không tồn tại");

                var currentUserEmail = _currentUserService.Email;
                if (string.IsNullOrWhiteSpace(currentUserEmail))
                    throw new Exception("Không thể xác định người dùng hiện tại");

                // Cập nhật các trường nếu có giá trị
                if (!string.IsNullOrWhiteSpace(request.Name))
                    entity.Name = request.Name;

                if (request.Description != null)
                    entity.Description = request.Description;

                if (request.RefundPercentage.HasValue)
                {
                    if (request.RefundPercentage.Value < 0 || request.RefundPercentage.Value > 100)
                        throw new Exception("Tỷ lệ hoàn tiền phải từ 0 đến 100");
                    entity.RefundPercentage = request.RefundPercentage.Value;
                }

                entity.UpdatedAt = DateTime.UtcNow;
                entity.UpdatedBy = currentUserEmail;

                await _refundPolicyRepository.UpdateAsync(entity);
                return _mapper.Map<RefundPolicyDto>(entity);
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi cập nhật chính sách hoàn tiền: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Cập nhật trạng thái IsActive của RefundPolicy
        /// </summary>
        public async Task<RefundPolicyDto> UpdateIsActiveAsync(int id, bool isActive)
        {
            try
            {
                if (id <= 0)
                    throw new Exception("Id phải lớn hơn 0");

                var entity = await _refundPolicyRepository.GetByIdAsync(id);
                if (entity == null)
                    throw new Exception("Chính sách hoàn tiền không tồn tại");

                var currentUserEmail = _currentUserService.Email;
                if (string.IsNullOrWhiteSpace(currentUserEmail))
                    throw new Exception("Không thể xác định người dùng hiện tại");

                entity.IsActive = isActive;
                entity.UpdatedAt = DateTime.UtcNow;
                entity.UpdatedBy = currentUserEmail;

                await _refundPolicyRepository.UpdateAsync(entity);
                return _mapper.Map<RefundPolicyDto>(entity);
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi cập nhật trạng thái chính sách hoàn tiền: {ex.Message}", ex);
            }
        }
    }
}

