using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Requests;
using EduMatch.DataAccessLayer.Enum;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace EduMatch.PresentationLayer.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class CloudMediaController : ControllerBase
	{
		//private readonly ICloudMediaService _cloudService;

		//public CloudMediaController(ICloudMediaService cloudService)
		//{
		//	_cloudService = cloudService;
		//}

		///// <summary>
		///// Upload file từ máy người dùng (multipart/form-data)
		///// </summary>
		//[HttpPost("upload")]
		//[Consumes("multipart/form-data")] // 👈 bắt buộc để swagger biết đây là form upload
		//public async Task<IActionResult> Upload([FromForm] UploadMediaFormRequest request)
		//{
		//	if (request.File == null || request.File.Length == 0)
		//		return BadRequest("File rỗng hoặc không tồn tại");

		//	using var stream = request.File.OpenReadStream();

		//	var uploadRequest = new UploadToCloudRequest(
		//		Content: stream,
		//		FileName: request.File.FileName,
		//		ContentType: request.File.ContentType ?? "application/octet-stream",
		//		LengthBytes: request.File.Length,
		//		OwnerEmail: request.OwnerEmail,
		//		MediaType: request.MediaType
		//	);

		//	var result = await _cloudService.UploadAsync(uploadRequest);
		//	return Ok(result);
		//}

		///// <summary>
		///// Upload file từ một URL có sẵn (link ảnh hoặc video)
		///// </summary>
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

		///// <summary>
		///// Xoá file theo publicId
		///// </summary>
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
	/// DTO upload file từ form-data
	/// </summary>
	//public class UploadMediaFormRequest
	//{
	//	[Required]
	//	public IFormFile File { get; set; } = default!;

	//	[Required]
	//	public string OwnerEmail { get; set; } = string.Empty;

	//	[Required]
	//	public MediaType MediaType { get; set; }
	//}

	///// <summary>
	///// DTO hỗ trợ upload từ URL
	///// </summary>
	//public class UploadFromUrlDto
	//{
	//	public string FileUrl { get; set; } = string.Empty;
	//	public string OwnerEmail { get; set; } = string.Empty;
	//}
}
