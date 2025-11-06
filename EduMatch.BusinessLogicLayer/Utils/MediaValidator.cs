using EduMatch.BusinessLogicLayer.Requests.Common;
using EduMatch.BusinessLogicLayer.Settings;
using EduMatch.DataAccessLayer.Enum;
using Microsoft.Extensions.Options;

namespace EduMatch.BusinessLogicLayer.Utils
{
	public sealed class MediaValidator : Interfaces.IMediaValidator
	{
		private readonly IOptionsMonitor<CloudinaryRootOptions> _monitor;

		public MediaValidator(IOptionsMonitor<CloudinaryRootOptions> monitor)
		{
			_monitor = monitor;
		}


		public Task ValidateAsync(UploadToCloudRequest request, CancellationToken ct = default)
		{
			if (request is null) throw new ArgumentNullException(nameof(request));
			if (request.Content is null || request.LengthBytes <= 0)
				throw new InvalidOperationException("File content is empty.");

			var policy = _monitor.CurrentValue.UploadPolicy;

			switch (request.MediaType)
			{
				case MediaType.Image:
					ValidateFileBasic(
						request.ContentType,
						request.FileName,
						policy.Image.AllowedContentTypes,
						policy.Image.AllowedExtensions,
						maxSizeMb: policy.Image.MaxSizeMB,
						lengthBytes: request.LengthBytes
					);
					break;

				case MediaType.Video:
					ValidateFileBasic(
						request.ContentType,
						request.FileName,
						policy.Video.AllowedContentTypes,
						policy.Video.AllowedExtensions,
						maxSizeMb: policy.Video.MaxSizeMB,
						lengthBytes: request.LengthBytes
					);
					break;

				default:
					throw new NotSupportedException("Unsupported media type.");
			}

			return Task.CompletedTask;
		}

		public Task ValidateRemoteAsync(string fileUrl, string? contentType, MediaType mediaType, CancellationToken ct = default)
		{
			if (string.IsNullOrWhiteSpace(fileUrl))
				throw new ArgumentException("fileUrl is required.", nameof(fileUrl));

			var policy = _monitor.CurrentValue.UploadPolicy;
			var ext = Path.GetExtension(new Uri(fileUrl).AbsolutePath);

			switch (mediaType)
			{
				case MediaType.Image:
					ValidateFileBasic(
						contentType,
						ext,
						policy.Image.AllowedContentTypes,
						policy.Image.AllowedExtensions,
						maxSizeMb: policy.Image.MaxSizeMB,
						lengthBytes: null
					);
					break;

				case MediaType.Video:
					ValidateFileBasic(
						contentType,
						ext,
						policy.Video.AllowedContentTypes,
						policy.Video.AllowedExtensions,
						maxSizeMb: policy.Video.MaxSizeMB,
						lengthBytes: null
					);
					break;

				default:
					throw new NotSupportedException("Unsupported media type.");
			}

			return Task.CompletedTask;
		}

		private static void ValidateFileBasic(
			string? contentType,
			string fileNameOrExt,
			string[] allowedContentTypes,
			string[] allowedExtensions,
			int maxSizeMb,
			long? lengthBytes)
		{
			if (allowedContentTypes?.Length > 0 && !string.IsNullOrWhiteSpace(contentType) &&
				!allowedContentTypes.Contains(contentType, StringComparer.OrdinalIgnoreCase))
				throw new InvalidOperationException($"Content-Type '{contentType}' is not allowed.");

			var ext = fileNameOrExt.StartsWith(".") ? fileNameOrExt : Path.GetExtension(fileNameOrExt);
			if (allowedExtensions?.Length > 0 && !string.IsNullOrWhiteSpace(ext) &&
				!allowedExtensions.Contains(ext.TrimStart('.'), StringComparer.OrdinalIgnoreCase) &&
				!allowedExtensions.Contains(ext, StringComparer.OrdinalIgnoreCase))
				throw new InvalidOperationException($"Extension '{ext}' is not allowed.");

			if (lengthBytes.HasValue)
			{
				var maxBytes = (long)maxSizeMb * 1024 * 1024;
				if (lengthBytes.Value > maxBytes)
					throw new InvalidOperationException($"File too large. Max: {maxSizeMb} MB.");
			}
		}
	}
}
