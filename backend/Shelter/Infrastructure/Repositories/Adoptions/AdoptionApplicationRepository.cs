using Domain.Entities;
using Infrastructure.Data;
using Infrastructure.Repositories.Abstractions.Adoptions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories.Adoptions
{
    public class AdoptionApplicationRepository : GenericRepository<AdoptionApplication>,
        IAdoptionApplicationRepository
    {
        private readonly AppDbContext _context;

        public AdoptionApplicationRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<AdoptionApplication?> GetByIdWithStatusAsync(int id, CancellationToken ct = default)
        {
            return await _context.AdoptionApplication
                .AsNoTracking()
                .Include(x => x.AdoptionStatus)
                .FirstOrDefaultAsync(x => x.Id == id, ct);
        }

        public async Task<AdoptionApplication?> GetFullAsync(int id, CancellationToken ct = default)
        {
            return await _context.AdoptionApplication
                .AsNoTracking()
                .Include (x => x.AdoptionStatus)
                .Include(x => x.HomeVisit).ThenInclude(x => x.HomeVisitResult)
                .Include(x => x.Contract)
                .FirstOrDefaultAsync(x => x.Id == id, ct);
        }
    }
}
