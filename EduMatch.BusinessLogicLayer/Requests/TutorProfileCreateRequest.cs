using EduMatch.DataAccessLayer.Enum;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Requests
{
	public class TutorProfileCreateRequest
	{
		[Required(ErrorMessage = "User email is required.")]
		[EmailAddress(ErrorMessage = "Invalid email format.")]
		public string UserEmail { get; set; } = null!;

		[MaxLength(2000, ErrorMessage = "Bio cannot exceed 2000 characters.")]
		public string? Bio { get; set; }

		[MaxLength(2000, ErrorMessage = "Teaching experience cannot exceed 2000 characters.")]
		public string? TeachingExp { get; set; }

		[Required(ErrorMessage = "Video URL must be a valid URL.")]
		public IFormFile VideoIntro { get; set; }



		[Required(ErrorMessage = "Teaching mode is required.")]
		[EnumDataType(typeof(TeachingMode))]
		public TeachingMode TeachingModes { get; set; }

	}
}
