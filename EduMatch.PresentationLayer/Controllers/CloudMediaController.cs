using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Requests.Cloudinary;
using EduMatch.BusinessLogicLayer.Requests.Common;
using EduMatch.BusinessLogicLayer.Services;
using EduMatch.BusinessLogicLayer.Settings;
using EduMatch.BusinessLogicLayer.Utils;
using EduMatch.DataAccessLayer.Enum;
using EduMatch.PresentationLayer.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;

namespace EduMatch.PresentationLayer.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class CloudMediaController : ControllerBase
	{
		private readonly ICloudMediaService _cloudService;
		private readonly CurrentUserService _currentUserService;
		private readonly IMediaValidator _mediaValidator;
		private readonly GoogleCalendarSettings _googleCalendarSettings;

		public CloudMediaController(ICloudMediaService cloudService, CurrentUserService currentUserService, IMediaValidator mediaValidator, IOptions<GoogleCalendarSettings> googleCalendarSettings)
		{
			_cloudService = cloudService;
			_currentUserService = currentUserService;
			_mediaValidator = mediaValidator;
			_googleCalendarSettings = googleCalendarSettings.Value;
		}

		/// <summary>
		/// Upload media file to cloud storage
		/// </summary>
		[HttpPost("upload")]
		[Consumes("multipart/form-data")]
		public async Task<IActionResult> Upload([FromForm] UploadMediaFormRequest request, CancellationToken ct)
		{
			try
			{
				if (request?.File == null || request.File.Length == 0)
					return BadRequest(ApiResponse<string>.Fail("File rỗng hoặc không tồn tại"));

				// Lấy email hệ thống từ GoogleCalendarSettings
				var ownerEmail = _googleCalendarSettings.SystemAccountEmail;
				if (string.IsNullOrWhiteSpace(ownerEmail))
					return BadRequest(ApiResponse<string>.Fail("Email hệ thống không được cấu hình"));

				// Mở stream 1 lần và dùng nó cho validate + upload
				await using var stream = request.File.OpenReadStream();

				var uploadRequest = new UploadToCloudRequest(
					Content: stream,
					FileName: request.File.FileName,
					ContentType: request.File.ContentType ?? "application/octet-stream",
					LengthBytes: request.File.Length,
					OwnerEmail: ownerEmail,
					MediaType: request.MediaType
				);

				// Gọi validator (sẽ đọc config từ IOptionsMonitor bên trong validator)
				await _mediaValidator.ValidateAsync(uploadRequest, ct);

				//  đảm bảo pointer ở đầu trước khi upload
				if (stream.CanSeek) stream.Position = 0;

				// Gọi service upload (service có thể gọi validator lại; chấp nhận được)
				var result = await _cloudService.UploadAsync(uploadRequest, ct);

				if (result.Ok)
					return Ok(ApiResponse<object>.Ok(result, "Upload thành công"));

				return BadRequest(ApiResponse<string>.Fail(result.ErrorMessage ?? "Upload thất bại"));

			}
			catch (InvalidOperationException ioe)
			{
				// validator sẽ ném InvalidOperationException cho các rule invalid
				return BadRequest(ApiResponse<string>.Fail(ioe.Message));
			}
			catch (Exception ex)
			{
				// fallback
				return StatusCode(500, ApiResponse<string>.Fail("Đã xảy ra lỗi khi upload file", ex.Message));
			}
		}

	}

	/// <summary>
	/// Upload file từ một URL có sẵn (link ảnh hoặc video)
	/// </summary>
	//[HttpPost("upload-from-url")]
	//public async Task<IActionResult> UploadFromUrl([FromBody] UploadFromUrlDto dto)
	//{
	//	if (string.IsNullOrWhiteSpace(dto.FileUrl))
	//		return BadRequest("FileUrl không hợp lệ");

	//	var result = await _cloudService.UploadFromUrlAsync(
	//		dto.FileUrl,
	//		dto.OwnerEmail
	//	);

	//	return Ok(result);
	//}

	/// <summary>
	/// Xoá file theo publicId
	/// </summary>
	//[HttpDelete("{publicId}")]
	//public async Task<IActionResult> Delete(string publicId, [FromQuery] MediaType mediaType)
	//{
	//	if (string.IsNullOrWhiteSpace(publicId))
	//		return BadRequest("Thiếu publicId");

	//	var result = await _cloudService.DeleteByPublicIdAsync(publicId, mediaType);
	//	return Ok(result);
	//}
}


	

	/// <summary>
	/// DTO hỗ trợ upload từ URL
	/// </summary>
	//public class UploadFromUrlDto
	//{
	//	public string FileUrl { get; set; } = string.Empty;
	//	public string OwnerEmail { get; set; } = string.Empty;
	//}

