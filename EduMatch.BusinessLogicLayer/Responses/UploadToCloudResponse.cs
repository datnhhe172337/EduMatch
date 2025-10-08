using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Responses
{
	public sealed record UploadToCloudResponse(
		bool Ok,
		string? PublicId,
		string? SecureUrl,
		string? ResourceType,
		string? ErrorMessage
	);
}
