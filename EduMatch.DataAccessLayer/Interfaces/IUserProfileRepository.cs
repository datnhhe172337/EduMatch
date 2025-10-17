using EduMatch.DataAccessLayer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.DataAccessLayer.Interfaces
{
	public interface  IUserProfileRepository
	{
		Task<UserProfile?> GetByEmailAsync(string email);
		Task UpdateAsync(UserProfile profile);
	}
}
