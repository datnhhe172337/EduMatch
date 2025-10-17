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
	public class TutorProfileUpdateRequest
	{
		[Required]
		[Range(1, int.MaxValue, ErrorMessage = "ID must be greater than 0.")]
		public int Id { get; set; }

		[MaxLength(2000, ErrorMessage = "Bio cannot exceed 2000 characters.")]
		public string? Bio { get; set; }

		[MaxLength(2000, ErrorMessage = "Teaching experience cannot exceed 2000 characters.")]
		public string? TeachingExp { get; set; }

		public IFormFile? VideoIntro { get; set; }

		[Url(ErrorMessage = "Video URL must be a valid URL.")]
		public string? VideoIntroUrl { get; set; }

		
		[EnumDataType(typeof(TeachingMode), ErrorMessage = "Invalid TeachingMode")]
		public TeachingMode? TeachingModes { get; set; }


		[EnumDataType(typeof(TutorStatus), ErrorMessage = "Invalid TutorStatus")]
		public TutorStatus? Status { get; set; }
	}

}

