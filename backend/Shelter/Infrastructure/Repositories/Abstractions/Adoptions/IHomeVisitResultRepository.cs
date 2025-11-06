using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories.Abstractions.Adoptions
{
    public interface IHomeVisitResultRepository
    {
        Task<HomeVisitResult?> GetByCodeAsync(string code, CancellationToken ct = default);
        Task<HomeVisitResult?> GetByIdAsync(int id, CancellationToken ct = default);
    }
}
