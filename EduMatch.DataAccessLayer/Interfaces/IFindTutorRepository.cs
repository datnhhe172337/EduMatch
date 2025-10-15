using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Enum;
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
        Task<IEnumerable<TutorProfile>> SearchTutorsAsync(string? keyword, Gender? gender, string? city, TeachingMode? teachingMode, TutorStatus? statusId, int page, int pageSize);
    }
}
