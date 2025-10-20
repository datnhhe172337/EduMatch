using EduMatch.DataAccessLayer.Enum;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Requests.TutorProfile
{
	public class TutorProfileCreateRequest
	{

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

		public IFormFile? AvatarFile { get; set; }

		[Required]
		[Range(1, int.MaxValue, ErrorMessage = "ProvinceId must be a positive number.")]
		public int? ProvinceId { get; set; }

		[Required]
		[Range(1, int.MaxValue, ErrorMessage = "SubDistrictId must be a positive number.")]
		public int? SubDistrictId { get; set; }


		[MaxLength(2000, ErrorMessage = "Teaching experience cannot exceed 2000 characters.")]
		public string? TeachingExp { get; set; }
		

		// Either provide a file or a URL (e.g., YouTube). Only one is required.
		public IFormFile? VideoIntro { get; set; }


		[Url(ErrorMessage = "Video URL must be a valid URL.")]
		public string? VideoIntroUrl { get; set; }


		[Required(ErrorMessage = "Teaching mode is required.")]
		[EnumDataType(typeof(TeachingMode))]
		public TeachingMode TeachingModes { get; set; }



	}
}
