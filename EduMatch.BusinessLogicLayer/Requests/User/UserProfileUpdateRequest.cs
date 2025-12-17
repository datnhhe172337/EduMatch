using EduMatch.DataAccessLayer.Enum;
using System;
using System.ComponentModel.DataAnnotations;

namespace EduMatch.BusinessLogicLayer.Requests.User
{
	public class UserProfileUpdateRequest
	{
		[Required(ErrorMessage = "Email is required")]
		[EmailAddress(ErrorMessage = "Invalid email address")]
		public string UserEmail { get; set; } = null!;

		[StringLength(100, ErrorMessage = "Username cannot exceed 100 characters")]
		public string? UserName { get; set; }

		//[Phone(ErrorMessage = "Invalid phone number")]
		//[RegularExpression(@"^(?:0\d{9}|(?:\+?84)\d{9,10})$", ErrorMessage = "Phone is not a valid VN number.")]
		public string? Phone { get; set; }

		[DataType(DataType.Date)]
		public DateTime? Dob { get; set; }

		[EnumDataType(typeof(Gender))]		
		public Gender? Gender { get; set; }

		[Url(ErrorMessage = "Invalid URL format")]
		public string? AvatarUrl { get; set; }

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
