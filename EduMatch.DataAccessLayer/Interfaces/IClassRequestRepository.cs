using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.DataAccessLayer.Interfaces
{
    public interface IClassRequestRepository
    {
        Task<ClassRequest> CreateRequestToOpenClassAsync(ClassRequest request, List<ClassRequestSlotsAvailability> slots);

        Task<ClassRequest?> GetClassRequestByIdAsync(int classRequestId);

        Task UpdateRequestToOpenClassAsync(ClassRequest request, List<ClassRequestSlotsAvailability> slots);

        Task DeleteRequestToOpenClassAsync(ClassRequest request);

        Task<List<ClassRequest>> GetListOfAllOpenClassRequestsAsync();

        Task<List<ClassRequest>> GetClassRequestsByLearnerEmailandStatusAsync(string learnerEmail, ClassRequestStatus status);

        Task<List<ClassRequest>> GetClassRequestsByLearnerEmailAsync(string learnerEmail);

        Task<List<ClassRequest>> GetPendingClassRequestsAsync();

        Task UpdateStatusAsync(ClassRequest request);

        Task GetRequestsToExpireAsync();
    }
}
