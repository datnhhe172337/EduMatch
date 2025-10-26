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
	public class UserProfileUpdateRequest
	{       

        [Required, EmailAddress(ErrorMessage = "Invalid email address")]
		public string UserEmail { get; set; } = null!;

		[DataType(DataType.Date)]
		public DateTime? Dob { get; set; }

		[EnumDataType(typeof(Gender))]		
		public Gender? Gender { get; set; }

		[Url(ErrorMessage = "Invalid URL format")]
		public string? AvatarUrl { get; set; }

		public string? AvatarUrlPublicId { get; set; }

		[Range(1, int.MaxValue, ErrorMessage = "CityId must be a positive integer")]
		public int? CityId { get; set; }

		[Range(1, int.MaxValue, ErrorMessage = "SubDistrictId must be a positive integer")]
		public int? SubDistrictId { get; set; }

		[StringLength(255, ErrorMessage = "Address cannot exceed 255 characters")]
		public string? AddressLine { get; set; }

		[Range(-90, 90, ErrorMessage = "Latitude must be between -90 and 90")]
		public decimal? Latitude { get; set; }

		[Range(-180, 180, ErrorMessage = "Longitude must be between -180 and 180")]
		public decimal? Longitude { get; set; }

    }

}
