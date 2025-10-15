using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.DataAccessLayer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Interfaces
{
    public interface IFindTutorService
    {
        Task<IEnumerable<TutorProfile>> GetAllTutorsAsync();
        Task<IEnumerable<TutorProfile>> SearchTutorsAsync(TutorFilterDto filter);
    }
}
