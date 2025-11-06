using Domain.Entities;
using Infrastructure.Data;
using Infrastructure.Repositories.Abstractions;
using Infrastructure.Repositories.Abstractions.Adoptions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories.Adoptions
{
    public class AdoptionStatusRepository : IAdoptionStatusRepository
    {
        private AppDbContext _context;
        public AdoptionStatusRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<AdoptionStatus?> GetByCodeAsync(string code, CancellationToken ct = default)
        {
            return await _context.AdoptionStatus
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Code == code, ct);
        }
    }
}
