using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Services
{
    public class FavoriteTutorService : IFavoriteTutorService
    {
        private readonly IFavoriteTutorRepository _favoriteTutorRepository;

        public FavoriteTutorService(IFavoriteTutorRepository favoriteTutorRepository)
        {
            _favoriteTutorRepository = favoriteTutorRepository;
        }

        public async Task<bool> AddToFavoriteAsync(string userEmail, int tutorId)
        {
            return await _favoriteTutorRepository.AddFavoriteAsync(userEmail, tutorId);
        }

        public async Task<bool> RemoveFromFavoriteAsync(string userEmail, int tutorId)
        {
            return await _favoriteTutorRepository.RemoveFavoriteAsync(userEmail, tutorId);
        }

        public async Task<bool> IsFavoriteAsync(string userEmail, int tutorId)
        {
            return await _favoriteTutorRepository.IsFavoriteAsync(userEmail, tutorId);
        }

        public async Task<List<TutorProfile>> GetFavoriteTutorsAsync(string userEmail)
        {
            return await _favoriteTutorRepository.GetFavoriteTutorsAsync(userEmail);
        }
    }
}
