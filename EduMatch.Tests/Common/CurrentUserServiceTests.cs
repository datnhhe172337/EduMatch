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
		public CurrentUserServiceFake(string email)
			: base(null!)
		{
			Email = email;
			
		}

	
		public  string Email { get; }

	}
}
