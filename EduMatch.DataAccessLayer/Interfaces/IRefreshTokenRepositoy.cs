using EduMatch.DataAccessLayer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.DataAccessLayer.Interfaces
{
    public interface IRefreshTokenRepositoy
    {
        Task<RefreshToken?> ExistingRefreshTokenAsync(User user);

        Task<RefreshToken?> GetValidRefreshTokenAsync(string tokenHash);
        Task<RefreshToken?> GetByTokenHashAsync(string tokenHash);

        Task CreateRefreshTokenAsync(RefreshToken refreshToken);

        Task UpdateRefreshTokenAsync(RefreshToken refreshToken);


    }
}
