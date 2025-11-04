using EduMatch.DataAccessLayer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.DataAccessLayer.Interfaces
{
    public interface ISystemFeeRepository
    {
        Task<SystemFee?> GetCurrentActiveFeeAsync();
    }
}
