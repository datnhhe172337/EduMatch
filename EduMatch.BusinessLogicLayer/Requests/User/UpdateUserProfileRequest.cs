using EduMatch.DataAccessLayer.Enum;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Requests.User
{
    
    public class UpdateUserProfileRequest
    {
        [MaxLength(100, ErrorMessage = "User name cannot exceed 100 characters.")]
        public string? UserName { get; set; }

        [MaxLength(30)]
        [RegularExpression(@"^(?:0\d{9}|(?:\+?84)\d{9,10})$", ErrorMessage = "Phone is not a valid VN number.")]
        public string? Phone { get; set; }

        [Required, EmailAddress(ErrorMessage = "Invalid email address")]
        public string UserEmail { get; set; } = null!;

        [DataType(DataType.Date)]
        public DateTime? Dob { get; set; }

        [EnumDataType(typeof(Gender))]
        public Gender? Gender { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "CountryId must be a positive integer")]
        public int? CityId { get; set; }

        
        public int? SubDistrictId { get; set; }

        
        public string? AddressLine { get; set; }

        public IFormFile? AvatarFile { get; set; }
    }
}
