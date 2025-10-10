using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Settings
{
	public sealed class CloudinaryRootOptions
	{
		public string Env { get; set; } = "prod";
		public string AppName { get; set; } = "EduMatch";

		public CloudinaryOptions Cloudinary { get; set; } = new();
		public NamingOptions Naming { get; set; } = new();
		public MediaDeliveryOptions MediaDelivery { get; set; } = new();
		public UploadPolicyOptions UploadPolicy { get; set; } = new();
	}

	public sealed class CloudinaryOptions
	{
		public string CloudName { get; set; } = "";
		public string ApiKey { get; set; } = "";
		public string ApiSecret { get; set; } = "";
		public string UploadPreset { get; set; } = "";
		public string AssetFolder { get; set; } = "user-infor";
	}

	public sealed class NamingOptions
	{
		public bool UseHashedEmail { get; set; } = true;
		public int OwnerKeyHexLength { get; set; } = 10;
		public int ShardBuckets { get; set; } = 16;
		public bool UseUtcClock { get; set; } = true;
		public string PublicIdPattern { get; set; } =
			"{env}/{app}/{type}/{shard}/{ownerKey}/{guid}";
	}

	public sealed class MediaDeliveryOptions
	{
		public ImageOptions Image { get; set; } = new();
		public VideoOptions Video { get; set; } = new();

		public sealed class ImageOptions
		{
			public int StoreMaxWidth { get; set; } = 1600;
			public int DefaultWidth { get; set; } = 800;
			public int DefaultHeight { get; set; } = 450;
			public string DefaultCrop { get; set; } = "fill";
		}
		public sealed class VideoOptions
		{
			public int PosterWidth { get; set; } = 800;
			public int PosterHeight { get; set; } = 450;
		}
	}

	public sealed class UploadPolicyOptions
	{
		public FileImagePolicy Image { get; set; } = new();
		public FileVideoPolicy Video { get; set; } = new();

		public sealed class FileImagePolicy
		{
			public int MaxSizeMB { get; set; } = 5;
			public string[] AllowedContentTypes { get; set; } = Array.Empty<string>();
			public string[] AllowedExtensions { get; set; } = Array.Empty<string>();
		}
		public sealed class FileVideoPolicy
		{
			public int MaxSizeMB { get; set; } = 50;
			public string[] AllowedContentTypes { get; set; } = Array.Empty<string>();
			public string[] AllowedExtensions { get; set; } = Array.Empty<string>();
		}
	}
}
