using EduMatch.BusinessLogicLayer.Enum;
using EduMatch.BusinessLogicLayer.Requests;
using EduMatch.BusinessLogicLayer.Settings;
using Microsoft.Extensions.Options;
using static System.Net.Mime.MediaTypeNames;

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
			throw new NotImplementedException();
		}

		public Task ValidateRemoteAsync(string fileUrl, string? contentType, MediaType mediaType, CancellationToken ct = default)
		{
			throw new NotImplementedException();
		}
	}
}
