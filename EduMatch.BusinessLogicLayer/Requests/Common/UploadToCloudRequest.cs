using EduMatch.DataAccessLayer.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Requests.Common
{
	public sealed record UploadToCloudRequest(
		Stream Content,
		string FileName,
		string ContentType,
		long LengthBytes,
		string OwnerEmail,
		MediaType MediaType
	);
}
