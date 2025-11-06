using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Requests;
using EduMatch.DataAccessLayer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Interfaces
{
    public interface IClassRequestService
    {
        Task<ClassRequest> CreateRequestToOpenClassAsync(ClassCreateRequest dto, string learnerEmail);
        Task<bool> UpdateClassRequestAsync(int classRequestId, UpdateClassRequest request, string learnerEmail);
        Task<ClassRequestDetailDto?> GetDetailsClassRequestByIdAsync(int classRequestId);
        Task<List<ClassRequestItemDto>> GetPendingClassRequestsAsync();
        Task<List<ClassRequestItemDto>> GetListAllClassRequestsByLearnerEmail(string learnerEmail);
        Task<List<ClassRequestItemDto>> GetPendingClassRequestsByLearnerEmail(string learnerEmail);
        Task<List<ClassRequestItemDto>> GetOpenClassRequestsByLearnerEmail(string learnerEmail);
        Task<List<ClassRequestItemDto>> GetRejectedClassRequestsByLearnerEmail(string learnerEmail);
        Task<List<ClassRequestItemDto>> GetExpiredClassRequestsByLearnerEmail(string learnerEmail);
        Task<List<ClassRequestItemDto>> GetCanceledClassRequestsByLearnerEmail(string learnerEmail);

        Task<List<ClassRequestItemDto>> GetOpenClassRequestsAsync();

        Task<bool> DeleteClassRequestAsync(int classRequestId, string learnerEmail);
        Task CancelClassRequestAsync(int classRequestId, string learnerEmail, CancelClassRequestDto dto);

        Task ApproveOrRejectClassRequestAsync(int classRequestId, string businessAdEmail, IsApprovedClassRequestDto dto);
        Task ExpireClassRequestsAsync();

    }
}
