using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories.Abstractions.Adoptions
{
    public interface IAdoptionContractRepository : IGenericRepository<AdoptionContract>
    {
        Task<AdoptionContract?> GetByApplicationIdAsync(int appId, CancellationToken ct = default);
        Task<AdoptionContract?> GetByVerificationContentAsync(string content, CancellationToken ct = default);
    }
}
