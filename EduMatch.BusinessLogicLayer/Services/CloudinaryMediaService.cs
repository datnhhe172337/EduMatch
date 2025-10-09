using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using EduMatch.BusinessLogicLayer.Enum;
using EduMatch.BusinessLogicLayer.Helper;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Requests;
using EduMatch.BusinessLogicLayer.Responses;
using EduMatch.BusinessLogicLayer.Settings;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Services
{
	public sealed class CloudinaryMediaService : ICloudMediaService
	{
		private readonly IOptionsMonitor<CloudinaryRootOptions> _monitor;
		private readonly IMediaValidator _validator;

		public CloudinaryMediaService(
			IOptionsMonitor<CloudinaryRootOptions> monitor,
			IMediaValidator validator)
		{
			_monitor = monitor;
			_validator = validator;
		}

		private Cloudinary CreateClient()
		{
			var opt = _monitor.CurrentValue.Cloudinary;
			var account = new Account(opt.CloudName, opt.ApiKey, opt.ApiSecret);
			var cloudinary = new Cloudinary(account)
			{
				Api = { Secure = true }
			};
			return cloudinary;
		}

		public async Task<UploadToCloudResponse> UploadAsync(UploadToCloudRequest request, CancellationToken ct = default)
		{
			try
			{
				await _validator.ValidateAsync(request, ct);

				var root = _monitor.CurrentValue;
				var naming = root.Naming;

				var typeSlug = request.MediaType == MediaType.Image ? "image" : "video";
				string publicId = CloudinaryNamingHelper.GeneratePublicId(
					env: root.Env,
					app: root.AppName,
					type: typeSlug,
					email: request.OwnerEmail,
					options: naming
				);

				// reset stream để Cloudinary đọc từ đầu
				if (request.Content.CanSeek) request.Content.Position = 0;

				var cloudinary = CreateClient();
				var folder = root.Cloudinary.AssetFolder?.Trim('/') ?? "uploads";

				RawUploadResult result;

				if (request.MediaType == MediaType.Image)
				{
					var up = new ImageUploadParams
					{
						File = new FileDescription(request.FileName, request.Content),
						// Cloudinary sẽ ghép: folder + "/" + public_id
						// Nếu đã set PublicId có path con, không cần folder. Ở đây tách rõ để dễ quản trị.
						Folder = folder,
						PublicId = publicId,
						UseFilename = false,
						UniqueFilename = false,
						Overwrite = false
					
					};
					result = await cloudinary.UploadAsync(up, ct);
				}
				else // Video
				{
					var up = new VideoUploadParams
					{
						File = new FileDescription(request.FileName, request.Content),
						Folder = folder,
						PublicId = publicId,
						UseFilename = false,
						UniqueFilename = false,
						Overwrite = false
					};
					result = await cloudinary.UploadAsync(up, ct);
				}

				if (result.StatusCode is System.Net.HttpStatusCode.OK or System.Net.HttpStatusCode.Created)
				{
					return new UploadToCloudResponse(
						Ok: true,
						PublicId: result.PublicId,
						SecureUrl: result.SecureUrl?.ToString(),
						ResourceType: result.ResourceType,
						ErrorMessage: null
					);
				}

				return new UploadToCloudResponse(false, null, null, null,
					$"Upload failed: {result.StatusCode} {result.Error?.Message}");
			}
			catch (Exception ex)
			{
				return new UploadToCloudResponse(false, null, null, null, ex.Message);
			}
		}

		public async Task<UploadToCloudResponse> UploadFromUrlAsync(
			string fileUrl,
			string fileName,
			string ownerEmail,
			MediaType mediaType,
			string? contentType = null,
			CancellationToken ct = default)
		{
			try
			{
				await _validator.ValidateRemoteAsync(fileUrl, contentType, mediaType, ct);

				var root = _monitor.CurrentValue;
				var naming = root.Naming;

				var typeSlug = mediaType == MediaType.Image ? "image" : "video";
				string publicId = CloudinaryNamingHelper.GeneratePublicId(
					env: root.Env,
					app: root.AppName,
					type: typeSlug,
					email: ownerEmail,
					options: naming
				);

				var cloudinary = CreateClient();
				var folder = root.Cloudinary.AssetFolder?.Trim('/') ?? "uploads";

				RawUploadResult result;
				if (mediaType == MediaType.Image)
				{
					var up = new ImageUploadParams
					{
						File = new FileDescription(fileUrl), // URL
						Folder = folder,
						PublicId = publicId,
						UseFilename = false,
						UniqueFilename = false,
						Overwrite = false
					};
					result = await cloudinary.UploadAsync(up, ct);
				}
				else
				{
					var up = new VideoUploadParams
					{
						File = new FileDescription(fileUrl),
						Folder = folder,
						PublicId = publicId,
						UseFilename = false,
						UniqueFilename = false,
						Overwrite = false
					};
					result = await cloudinary.UploadAsync(up, ct);
				}

				if (result.StatusCode is System.Net.HttpStatusCode.OK or System.Net.HttpStatusCode.Created)
				{
					return new UploadToCloudResponse(
						Ok: true,
						PublicId: result.PublicId,
						SecureUrl: result.SecureUrl?.ToString(),
						ResourceType: result.ResourceType,
						ErrorMessage: null
					);
				}

				return new UploadToCloudResponse(false, null, null, null,
					$"Remote upload failed: {result.StatusCode} {result.Error?.Message}");
			}
			catch (Exception ex)
			{
				return new UploadToCloudResponse(false, null, null, null, ex.Message);
			}
		}
	}
}
