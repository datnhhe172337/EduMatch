using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Interfaces
{
    public interface IEmbeddingService
    {
        Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken ct = default);
    }
}
