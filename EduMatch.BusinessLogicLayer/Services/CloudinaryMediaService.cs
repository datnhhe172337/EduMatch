using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using EduMatch.BusinessLogicLayer.Helper;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Requests.Common;
using EduMatch.BusinessLogicLayer.Responses.Common;
using EduMatch.BusinessLogicLayer.Settings;
using EduMatch.DataAccessLayer.Enum;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace EduMatch.BusinessLogicLayer.Services;

public sealed class CloudinaryMediaService : ICloudMediaService
{
	private readonly IOptionsMonitor<CloudinaryRootOptions> _monitor;
	private readonly IMediaValidator _validator;
	private readonly IHttpClientFactory _httpFactory;
	private readonly Cloudinary _cloudinary;

	public CloudinaryMediaService(
		IOptionsMonitor<CloudinaryRootOptions> monitor,
		IMediaValidator validator,
		IHttpClientFactory httpFactory,
		Cloudinary cloudinary) // <-- inject từ DI
	{
		_monitor = monitor;
		_validator = validator;
		_httpFactory = httpFactory;
		_cloudinary = cloudinary; // <-- gán field
	}

	public async Task<UploadToCloudResponse> UploadAsync(UploadToCloudRequest request, CancellationToken ct = default)
	{
		try
		{
			await _validator.ValidateAsync(request, ct);
			var root = _monitor.CurrentValue;

			var typeSlug = request.MediaType == MediaType.Image ? "image" : "video";
			string publicId = CloudinaryNamingHelper.GeneratePublicId(
				env: root.Env,
				app: root.AppName,
				type: typeSlug,
				email: request.OwnerEmail,
				options: root.Naming
			);

			Stream streamToUpload = request.Content;

			if (request.MediaType == MediaType.Image)
			{
				int maxWidth = root.MediaDelivery?.Image?.StoreMaxWidth > 0
					? root.MediaDelivery.Image.StoreMaxWidth
					: 1920; // fallback an toàn
				streamToUpload = ResizeImage(request.Content, maxWidth);
			}

			if (streamToUpload.CanSeek) streamToUpload.Position = 0;

			var folder = (root.Cloudinary.AssetFolder ?? "uploads").Trim('/');

			RawUploadResult result;
			if (request.MediaType == MediaType.Image)
			{
				var up = new ImageUploadParams
				{
					File = new FileDescription(request.FileName, streamToUpload),
					Folder = folder,
					PublicId = publicId,
					UseFilename = false,
					UniqueFilename = false,
					Overwrite = false
				};
				result = await _cloudinary.UploadAsync(up, ct);
			}
			else
			{
				var up = new VideoUploadParams
				{
					File = new FileDescription(request.FileName, streamToUpload),
					Folder = folder,
					PublicId = publicId,
					UseFilename = false,
					UniqueFilename = false,
					Overwrite = false
				};
				result = await _cloudinary.UploadAsync(up, ct);
			}

			if (result.StatusCode is System.Net.HttpStatusCode.OK or System.Net.HttpStatusCode.Created)
				return new UploadToCloudResponse(true, result.PublicId, result.SecureUrl?.ToString(), result.ResourceType, null);

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
	string ownerEmail,
	CancellationToken ct = default)
	{
		try
		{
			// SUY LUẬN fileName từ URL
			string fileName = Path.GetFileName(new Uri(fileUrl).AbsolutePath);
			if (string.IsNullOrWhiteSpace(fileName))
				fileName = $"auto_{Guid.NewGuid()}.jpg"; // fallback

			string ext = Path.GetExtension(fileName).ToLowerInvariant();
			MediaType mediaType = ext switch
			{
				".mp4" or ".webm" or ".mov" => MediaType.Video,
				_ => MediaType.Image // fallback mặc định
			};

			// HEAD để lấy content-type
			string? contentType = null;
			try
			{
				var client = _httpFactory.CreateClient();
				var head = await client.SendAsync(new HttpRequestMessage(System.Net.Http.HttpMethod.Head, fileUrl), ct);
				contentType = head.Content.Headers.ContentType?.MediaType;
			}
			catch (Exception ex)
			{
				
				throw new InvalidOperationException($" contentType : {ex.Message}", ex);
			}

			await _validator.ValidateRemoteAsync(fileUrl, contentType, mediaType, ct);

			var root = _monitor.CurrentValue;

			string typeSlug = mediaType == MediaType.Image ? "image" : "video";
			string publicId = CloudinaryNamingHelper.GeneratePublicId(
				env: root.Env,
				app: root.AppName,
				type: typeSlug,
				email: ownerEmail,
				options: root.Naming
			);

			string folder = (root.Cloudinary.AssetFolder ?? "uploads").Trim('/');

			RawUploadResult result;

			if (mediaType == MediaType.Image)
			{
				var remoteStream = await DownloadImageAsync(fileUrl, ct);
				int maxWidth = root.MediaDelivery?.Image?.StoreMaxWidth > 0
					? root.MediaDelivery.Image.StoreMaxWidth
					: 1920;
				var resized = ResizeImage(remoteStream, maxWidth);

				var up = new ImageUploadParams
				{
					File = new FileDescription(fileName, resized),
					Folder = folder,
					PublicId = publicId,
					UseFilename = false,
					UniqueFilename = false,
					Overwrite = false
				};

				result = await _cloudinary.UploadAsync(up, ct);
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

				result = await _cloudinary.UploadAsync(up, ct);
			}

			if (result.StatusCode is System.Net.HttpStatusCode.OK or System.Net.HttpStatusCode.Created)
				return new UploadToCloudResponse(true, result.PublicId, result.SecureUrl?.ToString(), result.ResourceType, null);

			return new UploadToCloudResponse(false, null, null, null,
				$"Remote upload failed: {result.StatusCode} {result.Error?.Message}");
		}
		catch (Exception ex)
		{
			return new UploadToCloudResponse(false, null, null, null, ex.Message);
		}
	}


	public async Task<UploadToCloudResponse> DeleteByPublicIdAsync(
		string publicId,
		MediaType mediaType,
		CancellationToken ct = default)
	{
		try
		{
			if (string.IsNullOrWhiteSpace(publicId))
				return new UploadToCloudResponse(false, null, null, null, "publicId is empty.");

			var resourceType = mediaType switch
			{
				MediaType.Image => ResourceType.Image,
				MediaType.Video => ResourceType.Video,
				_ => ResourceType.Image
			};
			publicId = Uri.UnescapeDataString(publicId);


			var delParams = new DeletionParams(publicId)
			{
				Type = "upload",
				Invalidate = true,
				ResourceType = resourceType
			};

			var result = await _cloudinary.DestroyAsync(delParams);
			var ok = string.Equals(result.Result, "ok", StringComparison.OrdinalIgnoreCase);

			var errorMessage = ok ? null : result.Error?.Message ?? $"Xoá thất bại: {result.Result}";

			return new UploadToCloudResponse(
				ok,
				publicId,
				null,
				resourceType.ToString().ToLowerInvariant(),
				ok ? null : result.Error?.Message
			);
		}
		catch (Exception ex)
		{
			return new UploadToCloudResponse(false, publicId, null, null, ex.Message);
		}
	}

	private Stream ResizeImage(Stream input, int maxWidth)
	{
		input.Position = 0;

		using var image = Image.Load<Rgba32>(input);
		if (image.Width <= maxWidth)
		{
			input.Position = 0;
			return input;
		}

		image.Mutate(x => x.Resize(new ResizeOptions
		{
			Mode = ResizeMode.Max,
			Size = new SixLabors.ImageSharp.Size(maxWidth, 0)
		}));

		var output = new MemoryStream();
		image.Save(output, new WebpEncoder { Quality = 90, FileFormat = WebpFileFormatType.Lossy });
		output.Position = 0;
		return output;
	}

	private async Task<Stream> DownloadImageAsync(string url, CancellationToken ct)
	{
		var client = _httpFactory.CreateClient(); // hoặc CreateClient("media-validator")
		using var response = await client.GetAsync(url, ct);
		response.EnsureSuccessStatusCode();

		await using var stream = await response.Content.ReadAsStreamAsync(ct);
		var memStream = new MemoryStream();
		await stream.CopyToAsync(memStream, ct);
		memStream.Position = 0;
		return memStream;
	}
}
