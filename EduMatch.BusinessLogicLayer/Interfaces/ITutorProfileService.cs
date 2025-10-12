using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.DataAccessLayer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Interfaces
{
    public interface ITutorProfileService
    {
        Task<TutorProfile?> GetByIdAsync(int id);

        Task<TutorProfile?> GetByEmailAsync(String email);
        Task<bool> UpdateTutorProfileAsync(string email, UpdateTutorProfileDto dto);
    }
}
