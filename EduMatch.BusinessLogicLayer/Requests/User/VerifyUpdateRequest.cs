using EduMatch.DataAccessLayer.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Requests.User
{
	public sealed class VerifyUpdateRequest
		{
			public int Id { get; set; }
			public VerifyStatus Verified { get; set; }
			public string? RejectReason { get; set; }
		}
}