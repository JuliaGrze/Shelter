using Domain.Entities;
using Infrastructure.Repositories.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories.Abstractions.Adoptions
{
    public interface IAdoptionApplicationRepository : IGenericRepository<AdoptionApplication>
    {
        //Wniosek adopcyjny ze statusem
        Task<AdoptionApplication?> GetByIdWithStatusAsync(int id, CancellationToken ct = default);

        // Winosek z: status + wizyta + wynik + kontrakt
        Task<AdoptionApplication?> GetFullAsync(int id, CancellationToken ct = default);

        //Lista wnioskow
        IQueryable<AdoptionApplication> QueryForList();
    }
}
