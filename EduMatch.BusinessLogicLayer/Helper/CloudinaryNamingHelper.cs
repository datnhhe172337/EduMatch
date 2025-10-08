using EduMatch.BusinessLogicLayer.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Helper
{
	public static class CloudinaryNamingHelper
	{
		public static (string OwnerKey, string Shard) GenerateOwnerKey(string email, NamingOptions options)
		{
			if (string.IsNullOrWhiteSpace(email))
				throw new ArgumentException("Email cannot be null or empty.", nameof(email));

			string ownerKey;
			if (options.UseHashedEmail)
			{
				using var sha = SHA256.Create();
				byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(email.ToLowerInvariant().Trim()));
				string hex = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
				ownerKey = hex[..Math.Min(options.OwnerKeyHexLength, hex.Length)];
			}
			else
			{
				ownerKey = email.Split('@')[0];
			}

			string shard = "";
			if (options.ShardBuckets > 0)
			{
				int shardIndex = Math.Abs(ownerKey.GetHashCode()) % options.ShardBuckets;
				shard = $"u{shardIndex:x}";
			}

			return (ownerKey, shard);
		}

		public static string GeneratePublicId(string env, string app, string type, string email, NamingOptions options)
		{
			var (ownerKey, shard) = GenerateOwnerKey(email, options);
			string guid = Guid.NewGuid().ToString("N");


			string publicId = options.PublicIdPattern
				.Replace("{env}", env)
				.Replace("{app}", app)
				.Replace("{type}", type)
				.Replace("{shard}", shard)
				.Replace("{ownerKey}", ownerKey)
				.Replace("{guid}", guid);

			return publicId;
		}
	}
}
