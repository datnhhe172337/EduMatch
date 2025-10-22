using EduMatch.DataAccessLayer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Interfaces
{
    public interface IFavoriteTutorService
    {
        Task<bool> AddToFavoriteAsync(string userEmail, int tutorId);
        Task<bool> RemoveFromFavoriteAsync(string userEmail, int tutorId);
        Task<bool> IsFavoriteAsync(string userEmail, int tutorId);
        Task<List<TutorProfile>> GetFavoriteTutorsAsync(string userEmail);
    }
}
