using EduMatch.BusinessLogicLayer.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Interfaces
{
    public interface IFeedbackService
    {
        Task<TutorFeedbackDto> CreateFeedbackAsync(CreateTutorFeedbackRequest request, string learnerEmail);
    }

}
