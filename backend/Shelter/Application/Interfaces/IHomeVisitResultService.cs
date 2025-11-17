using Application.Dtos.Adoptions.HomeVisit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IHomeVisitResultService
    {
        Task<List<HomeVisitResultDto>> ListAsync(CancellationToken ct = default);
        Task<HomeVisitResultDto?> GetByCodeAsync(string code, CancellationToken ct = default);
    }
}
