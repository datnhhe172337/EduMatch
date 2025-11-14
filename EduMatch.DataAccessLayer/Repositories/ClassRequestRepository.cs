using Azure.Core;
using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Enum;
using EduMatch.DataAccessLayer.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.DataAccessLayer.Repositories
{
    public class ClassRequestRepository : IClassRequestRepository
    {
        private readonly EduMatchContext _context;
        public ClassRequestRepository(EduMatchContext context)
        {
            _context = context;
        }

        public async Task<ClassRequest> CreateRequestToOpenClassAsync(ClassRequest request, List<ClassRequestSlotsAvailability> slots)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                _context.ClassRequests.Add(request);
                await _context.SaveChangesAsync();

                if (slots != null && slots.Count > 0)
                {
                    foreach (var slot in slots)
                        slot.ClassRequestId = request.Id;

                    _context.ClassRequestSlotsAvailabilities.AddRange(slots);
                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();
                return request;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task DeleteRequestToOpenClassAsync(ClassRequest request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                _context.ClassRequests.Remove(request);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
            
        }

        public async Task<ClassRequest?> GetClassRequestByIdAsync(int classRequestId)
        {
            return await _context.ClassRequests
                .Include(r => r.ClassRequestSlotsAvailabilities)
                  .ThenInclude(r => r.Slot)
                .Include(r => r.Subject)
                .Include(r => r.Level)
                .Include(r => r.Province)
                .Include(r => r.SubDistrict)
                .SingleOrDefaultAsync(x => x.Id == classRequestId);
        }

        public async Task<List<ClassRequest>> GetListOfAllOpenClassRequestsAsync()
        {
            return await _context.ClassRequests
                .Include(r => r.Subject)
                .Include(r => r.Level)
                .Include(t => t.LearnerEmailNavigation)
                   .ThenInclude(u => u.UserProfile)
                .Where(u => u.Status == ClassRequestStatus.Open)
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<ClassRequest>> GetClassRequestsByLearnerEmailandStatusAsync(string learnerEmail, ClassRequestStatus status)
        {
            return await _context.ClassRequests
                .Include(r => r.Subject)
                .Include(r => r.Level)
                .Include(t => t.LearnerEmailNavigation)
                   .ThenInclude(u => u.UserProfile)
                .Where(x => x.LearnerEmail == learnerEmail && x.Status == status)
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<ClassRequest>> GetClassRequestsByLearnerEmailAsync(string learnerEmail)
        {
            return await _context.ClassRequests
                .Include(r => r.Subject)
                .Include(r => r.Level)
                .Include(t => t.LearnerEmailNavigation)
                    .ThenInclude(u => u.UserProfile)
                .Where(x => x.LearnerEmail == learnerEmail)
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<ClassRequest>> GetPendingClassRequestsAsync()
        {
            return await _context.ClassRequests
               .Include(r => r.Subject)
               .Include(r => r.Level)
               .Include(t => t.LearnerEmailNavigation)
                   .ThenInclude(u => u.UserProfile)
               .Where(c => c.Status == ClassRequestStatus.Pending)
               .OrderBy(c => c.CreatedAt)
               .ToListAsync();
        }

        public async Task UpdateRequestToOpenClassAsync(ClassRequest request, List<ClassRequestSlotsAvailability> slots)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                _context.ClassRequests.Update(request);
                await _context.SaveChangesAsync();

                // Xóa slot cũ
                var oldSlots = _context.ClassRequestSlotsAvailabilities
                    .Where(s => s.ClassRequestId == request.Id);
                _context.ClassRequestSlotsAvailabilities.RemoveRange(oldSlots);
                await _context.SaveChangesAsync();

                if (slots != null && slots.Count > 0)
                {
                    foreach (var slot in slots)
                        slot.ClassRequestId = request.Id;

                    _context.ClassRequestSlotsAvailabilities.AddRange(slots);
                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task UpdateStatusAsync(ClassRequest request)
        {
            _context.ClassRequests.Update(request);
            await _context.SaveChangesAsync();
        }

        public async Task GetRequestsToExpireAsync()
        {
            var expirationDays = 14;

            var requestsToExpire = await _context.ClassRequests
                .Where(r => r.Status == ClassRequestStatus.Open
                         && r.ApprovedAt < DateTime.UtcNow.AddDays(-expirationDays))
                .ToListAsync();

            if (requestsToExpire.Any())
            {
                foreach (var r in requestsToExpire)
                {
                    r.Status = ClassRequestStatus.Expired;
                }

                await _context.SaveChangesAsync();
            }
        }
    }
}
