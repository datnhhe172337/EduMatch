using EduMatch.DataAccessLayer.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Requests
{
	public class UserProfileUpdateRequest
	{
		[Required, EmailAddress(ErrorMessage = "Invalid email address")]
		public string UserEmail { get; set; } = null!;

		public DateTime? Dob { get; set; }

		public Gender? Gender { get; set; }

		public string? AvatarUrl { get; set; }

		public string? AvatarUrlPublicId { get; set; }

		public int? CityId { get; set; }

		public int? SubDistrictId { get; set; }

		[StringLength(255, ErrorMessage = "Address cannot exceed 255 characters")]
		public string? AddressLine { get; set; }

		public decimal? Latitude { get; set; }

		public decimal? Longitude { get; set; }
	}

}
