using EduMatch.BusinessLogicLayer.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.Tests.Common
{
	public class CurrentUserServiceFake : CurrentUserService
	{
		private readonly string _email;

		public CurrentUserServiceFake(string email)
			: base(null!)
		{
			_email = email ?? string.Empty;
		}

		public override string Email => _email ?? string.Empty;
	}
}
