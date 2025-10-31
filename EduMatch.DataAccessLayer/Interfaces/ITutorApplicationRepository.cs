using EduMatch.DataAccessLayer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.DataAccessLayer.Interfaces
{
    public interface ITutorApplicationRepository
    {
        Task<bool> HasAppliedAsync(int classRequestId, int tutorId);
        Task AddApplicationAsync(TutorApplication app);
        Task<ClassRequest?> GetClassRequestByIdAsync(int classRequestId);

        Task<TutorProfile?> GetTutorByEmailAsync(string userEmail);

        Task<List<TutorApplication?>> GetApplicationsByClassRequestAsync(int classRequestId);
        Task<List<TutorApplication?>> GetApplicationsByTutorAsync(string tutorEmail);
        Task<List<TutorApplication?>> GetCanceledApplicationsByTutorAsync(string tutorEmail);
        Task<TutorApplication?> GetApplicationByIdAsync(int id);
        Task UpdateAsync(TutorApplication app);
    }
}
