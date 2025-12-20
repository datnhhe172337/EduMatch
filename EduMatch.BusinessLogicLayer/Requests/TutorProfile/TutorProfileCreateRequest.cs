using EduMatch.DataAccessLayer.Enum;
using System;
using System.ComponentModel.DataAnnotations;

namespace EduMatch.BusinessLogicLayer.Requests.TutorProfile
{
	public class TutorProfileCreateRequest
	{
	
		[Required, EmailAddress(ErrorMessage = "Invalid email address")]
		public string UserEmail { get; set; } = null!;

		[Required]
		[MaxLength(100, ErrorMessage = "User name cannot exceed 100 characters.")]
		public string? UserName { get; set; }

		[Required]
		[MaxLength(30)]
		[RegularExpression(@"^(?:0\d{9}|(?:\+?84)\d{9,10})$", ErrorMessage = "Phone is not a valid VN number.")]
		public string? Phone { get; set; }

		[MaxLength(2000, ErrorMessage = "Bio cannot exceed 2000 characters.")]
		public string? Bio { get; set; }


		[Required]
		[DataType(DataType.Date)]
		public DateTime? DateOfBirth { get; set; }


		[Url(ErrorMessage = "Avatar URL must be a valid URL.")]
		public string? AvatarUrl { get; set; }

		[Required]
		[Range(1, int.MaxValue, ErrorMessage = "ProvinceId must be a positive number.")]
		public int? ProvinceId { get; set; }

		[Required]
		[Range(1, int.MaxValue, ErrorMessage = "SubDistrictId must be a positive number.")]
		public int? SubDistrictId { get; set; }


		[MaxLength(2000, ErrorMessage = "Teaching experience cannot exceed 2000 characters.")]
		public string? TeachingExp { get; set; }
		

		[Url(ErrorMessage = "Video URL must be a valid URL.")]
		public string? VideoIntroUrl { get; set; }


		[Range(-90, 90, ErrorMessage = "Latitude must be between -90 and 90")]
		public decimal? Latitude { get; set; }

		[Range(-180, 180, ErrorMessage = "Longitude must be between -180 and 180")]
		public decimal? Longitude { get; set; }


		[Required(ErrorMessage = "Teaching mode is required.")]
		[EnumDataType(typeof(TeachingMode))]
		public TeachingMode TeachingModes { get; set; }

	}
}
