using EduMatch.DataAccessLayer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.DataAccessLayer.Interfaces
{
    public interface IFindTutorRepository
    {
        Task<IEnumerable<TutorProfile>> GetAllTutorsAsync();
        Task<IEnumerable<TutorProfile>> SearchTutorsAsync(string? keyword, string? gender, string? city, string? teachingMode, int? statusId, int page, int pageSize);
    }
}
