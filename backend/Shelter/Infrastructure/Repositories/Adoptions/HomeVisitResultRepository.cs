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
    public class HomeVisitResultRepository : IHomeVisitResultRepository
    {
        private readonly AppDbContext _context;
        public HomeVisitResultRepository(AppDbContext context)
        {
            _context = context;
        }

        public IQueryable<HomeVisitResult> Query() => _context.HomeVisitResult.AsNoTracking();

        public async Task<HomeVisitResult?> GetByCodeAsync(string code, CancellationToken ct = default)
        {
            return await _context.HomeVisitResult
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Code == code, ct);
        }

        public async Task<HomeVisitResult?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _context.HomeVisitResult
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id, ct);
        }

        public async Task<List<HomeVisitResult>> GetAllAsync(CancellationToken ct = default)
        {
            return await _context.HomeVisitResult.AsNoTracking()
                .OrderBy(x => x.Id)
                .ToListAsync(ct);
        }

        public async Task AddRangeIfMissingAsync(IEnumerable<HomeVisitResult> items, CancellationToken ct = default)
        {
            var existingCodes = await _context.HomeVisitResult.AsNoTracking()
                .Select(x => x.Code)
                .ToListAsync(ct);

            var toAdd = items.Where(i => !existingCodes.Contains(i.Code)).ToList();
            if (toAdd.Count == 0) return;

            await _context.HomeVisitResult.AddRangeAsync(toAdd, ct);
        }
    }
}
