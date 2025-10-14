using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.DataAccessLayer.Repositories
{
    public class RefreshTokenRepository : IRefreshTokenRepositoy
    {
        private readonly EduMatchContext _context;

        public RefreshTokenRepository(EduMatchContext context)
        {
            _context = context;
        }

        public async Task CreateRefreshTokenAsync(RefreshToken refreshToken)
        {
            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();
        }

        public async Task<RefreshToken?> ExistingRefreshTokenAsync(User user)
        {
            return await _context.RefreshTokens
                .SingleOrDefaultAsync(r => r.UserEmail == user.Email && r.ExpiresAt > DateTime.UtcNow && r.RevokedAt == null);
        }

        public async Task<RefreshToken?> GetValidRefreshTokenAsync(string tokenHash)
        {
            return await _context.RefreshTokens
                .Include(r => r.UserEmailNavigation)
                .SingleOrDefaultAsync(r =>
                    r.TokenHash == tokenHash &&
                    r.ExpiresAt > DateTime.UtcNow &&
                    r.RevokedAt == null);
        }

        public async Task<RefreshToken?> GetByTokenHashAsync(string tokenHash)
        {
            return await _context.RefreshTokens
                .Include(r => r.UserEmailNavigation)
                .SingleOrDefaultAsync(r => r.TokenHash == tokenHash);
        }


        public async Task UpdateRefreshTokenAsync(RefreshToken refreshToken)
        {
            _context.RefreshTokens.Update(refreshToken);
            await _context.SaveChangesAsync();
        }
    }
}
