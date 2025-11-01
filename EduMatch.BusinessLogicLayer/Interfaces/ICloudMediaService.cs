using EduMatch.BusinessLogicLayer.Requests.Common;
using EduMatch.BusinessLogicLayer.Responses.Common;
using EduMatch.DataAccessLayer.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Interfaces
{
	public interface ICloudMediaService
	{
		// upload using stream with request.MediaType
		Task<UploadToCloudResponse> UploadAsync(UploadToCloudRequest request, CancellationToken ct = default);


		// upload useing URL 
		Task<UploadToCloudResponse> UploadFromUrlAsync(
			string fileUrl,
			string ownerEmail,
			CancellationToken ct = default);

		// Xoá bằng publicId
		Task<UploadToCloudResponse> DeleteByPublicIdAsync(
			string publicId,
			MediaType mediaType,
			CancellationToken ct = default);

	}

	// validator cho media trước khi upload
	public interface IMediaValidator
	{
		Task ValidateAsync(UploadToCloudRequest request, CancellationToken ct = default);
		Task ValidateRemoteAsync(
			string fileUrl,
			string? contentType,
			MediaType mediaType,
			CancellationToken ct = default);
	}





}
