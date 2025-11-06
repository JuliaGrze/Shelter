using Infrastructure.Data;
using Infrastructure.Repositories.Abstractions.Adoptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories.Adoptions
{
    public class AdoptionUnitOfWork : IAdoptionUnitOfWork
    {
        private readonly AppDbContext _context;

        public AdoptionUnitOfWork(
            AppDbContext context,
            IAdoptionApplicationRepository adoptionApplications,
            IAdoptionStatusRepository adoptionStatuses,
            IHomeVisitResultRepository homeVisitResults,
            IAdoptionContractRepository adoptionContracts
            )
        {
            _context = context;
            AdoptionApplications = adoptionApplications;
            AdoptionStatuses = adoptionStatuses;
            HomeVisitResults = homeVisitResults;
            AdoptionContracts = adoptionContracts;
        }

        public IAdoptionApplicationRepository AdoptionApplications { get; }
        public IAdoptionStatusRepository AdoptionStatuses { get; }
        public IHomeVisitResultRepository HomeVisitResults { get; }
        public IAdoptionContractRepository AdoptionContracts { get; }

        public Task<int> SaveChangesAsync(CancellationToken ct = default)
            => _context.SaveChangesAsync(ct);

    }
}
