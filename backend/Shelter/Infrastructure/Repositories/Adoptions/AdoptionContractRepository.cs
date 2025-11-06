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
    public class AdoptionContractRepository : GenericRepository<AdoptionContract>,
        IAdoptionContractRepository
    {
        private readonly AppDbContext _context;

        public AdoptionContractRepository(AppDbContext context) : base(context) 
        {
            _context = context;
        }

        public async Task<AdoptionContract?> GetByApplicationIdAsync(int appId, CancellationToken ct = default)
        {
            return await _context.AdoptionContract
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.AdoptionApplicationId == appId, ct);
        }

        public async Task<AdoptionContract?> GetByVerificationContentAsync(string content, CancellationToken ct = default)
        {
            return await _context.AdoptionContract
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.VerificationQrContent == content, ct);
        }
    }
}
