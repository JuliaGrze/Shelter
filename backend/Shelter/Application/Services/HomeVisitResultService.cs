using Application.Dtos.Adoptions.HomeVisit;
using Application.Interfaces;
using Infrastructure.Repositories.Abstractions.Adoptions;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
    public class HomeVisitResultService : IHomeVisitResultService
    {
        private readonly IAdoptionUnitOfWork _uow;
        public HomeVisitResultService(IAdoptionUnitOfWork uow) => _uow = uow;

        public async Task<List<HomeVisitResultDto>> ListAsync(CancellationToken ct = default)
        {
            var list = await _uow.HomeVisitResults.Query()
                .OrderBy(x => x.Id)
                .Select(x => new HomeVisitResultDto
                {
                    Id = x.Id,
                    Code = x.Code,
                    Name = x.Name,
                    Description = x.Description,
                    IsFinal = x.IsFinal
                })
                .ToListAsync(ct);

            return list;
        }

        public async Task<HomeVisitResultDto?> GetByCodeAsync(string code, CancellationToken ct = default)
        {
            var x = await _uow.HomeVisitResults.GetByCodeAsync(code, ct);
            if (x == null) return null;

            return new HomeVisitResultDto
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                Description = x.Description,
                IsFinal = x.IsFinal
            };
        }
    }
}
