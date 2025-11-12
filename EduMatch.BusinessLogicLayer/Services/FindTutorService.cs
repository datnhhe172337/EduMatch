using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Interfaces;
using EduMatch.DataAccessLayer.Repositories;

namespace EduMatch.BusinessLogicLayer.Services
{
    public class FindTutorService : IFindTutorService
    {
        private readonly IFindTutorRepository _tutorRepo;

        public FindTutorService(IFindTutorRepository tutorRepo)
        {

            _tutorRepo = tutorRepo;
        }

        public async Task<IEnumerable<TutorProfile>> GetAllTutorsAsync()
        {
            return await _tutorRepo.GetAllTutorsAsync();
        }

        public async Task<IEnumerable<TutorProfile>> SearchTutorsAsync(TutorFilterDto filter)
        {
            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter));
            }
            return await _tutorRepo.SearchTutorsAsync(
                filter.Keyword,
                filter.Gender,
                filter.City,
                filter.TeachingMode,
                filter.StatusId,
                filter.Page,
                filter.PageSize
            );
        }
    }
}
