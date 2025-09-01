using Infrastructure.Data;
using Infrastructure.Repositories.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class UnitOfWork(AppDbContext appDbContext) : IUnitOfWork
    {
        private readonly AppDbContext _appDbContext = appDbContext;
        public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        {
            return await _appDbContext.SaveChangesAsync(ct);
        }
    }
}
