using EduMatch.DataAccessLayer.Enum;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Requests.Cloudinary
{
	public class UploadMediaFormRequest
	{
		[Required]
		public IFormFile File { get; set; } = default!;

		//[Required]
		//public string OwnerEmail { get; set; } = string.Empty;

		[Required]
		[EnumDataType(typeof(MediaType))]
		public MediaType MediaType { get; set; }
	}
}
