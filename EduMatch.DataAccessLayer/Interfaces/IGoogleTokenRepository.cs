using EduMatch.DataAccessLayer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.DataAccessLayer.Interfaces
{
	public interface IGoogleTokenRepository
	{
		/// <summary>
		/// Lấy GoogleToken theo email
		/// </summary>
		Task<GoogleToken?> GetByEmailAsync(string accountEmail);
		/// <summary>
		/// Cập nhật GoogleToken
		/// </summary>
		Task UpdateAsync(GoogleToken token);
		/// <summary>
		/// Tạo GoogleToken mới
		/// </summary>
		Task CreateAsync(GoogleToken token);
	}
}
