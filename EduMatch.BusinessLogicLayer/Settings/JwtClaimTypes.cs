using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Settings
{
	public static class JwtClaimTypes
	{
		public const string UserName = "email";
		public const string Email = "email";
		public const string FullName = "email";
		public const string PhoneNumber = "phone_number";
		public const string Address = "email";
		public const string Role = "role";
	}
}
