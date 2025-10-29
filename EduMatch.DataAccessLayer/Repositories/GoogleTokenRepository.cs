using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;


namespace EduMatch.DataAccessLayer.Repositories
{
	public class GoogleTokenRepository : IGoogleTokenRepository
	{
		private readonly EduMatchContext _context;
		public GoogleTokenRepository(EduMatchContext context) => _context = context;

		public async Task CreateAsync(GoogleToken token)
		{
			_context.GoogleTokens.AddAsync(token);
			await _context.SaveChangesAsync();
		}

		public async Task<GoogleToken?> GetByEmailAsync(string accountEmail)
		{
			return await _context.GoogleTokens
				.FirstOrDefaultAsync(x => x.AccountEmail == accountEmail);
		}

		public async Task UpdateAsync(GoogleToken token)
		{
			_context.GoogleTokens.Update(token);
			await _context.SaveChangesAsync();
		}
	}
}
