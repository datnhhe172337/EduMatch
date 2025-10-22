using EduMatch.DataAccessLayer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.DataAccessLayer.Interfaces
{
    public interface IFavoriteTutorRepository
    {
        Task<bool> AddFavoriteAsync(string userEmail, int tutorId);
        Task<bool> RemoveFavoriteAsync(string userEmail, int tutorId);
        Task<bool> IsFavoriteAsync(string userEmail, int tutorId);
        Task<List<TutorProfile>> GetFavoriteTutorsAsync(string userEmail);
    }
}
