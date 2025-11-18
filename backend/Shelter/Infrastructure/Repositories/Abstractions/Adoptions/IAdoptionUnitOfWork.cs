using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories.Abstractions.Adoptions
{
    public interface IAdoptionUnitOfWork : IUnitOfWork
    {
        IAdoptionApplicationRepository AdoptionApplications { get; }
        IAdoptionStatusRepository AdoptionStatuses { get; }
        IHomeVisitResultRepository HomeVisitResults { get; }
        IAdoptionContractRepository AdoptionContracts { get; }

        IGenericRepository<Animal> Animals { get; }

    }
}
