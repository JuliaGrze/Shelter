using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories.Abstractions.Adoptions
{
    public interface IAdoptionStatusRepository
    {
        Task<AdoptionStatus?> GetByCodeAsync(string code, CancellationToken ct = default);
    }
}
