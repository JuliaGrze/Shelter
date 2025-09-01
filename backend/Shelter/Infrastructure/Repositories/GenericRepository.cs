using Infrastructure.Data;
using Infrastructure.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly AppDbContext _context;
        public GenericRepository(AppDbContext appDbContext)
        {
            _context = appDbContext;
        }

        public async Task AddAsync(T entity, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(entity);
            await _context.Set<T>().AddAsync(entity, ct);
        }

        public void Delete(int id)
        {
            var entity = _context.Set<T>().FirstOrDefault(x => EF.Property<int>(x, "Id") == id);
            if (entity != null)
            {
                _context.Set<T>().Remove(entity);
            }
        }

        public async Task<List<T>> GetAllAsync(CancellationToken ct = default, params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> q = _context.Set<T>().AsNoTracking();
            foreach (var inc in includes) q = q.Include(inc);
            return await q.ToListAsync(ct);
        }

        public async Task<T?> GetByIdAsync(int id, CancellationToken ct = default, params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> q = _context.Set<T>().AsNoTracking();
            foreach(var inc in includes) q = q.Include(inc);
            return await q.FirstOrDefaultAsync(e => EF.Property<int>(e, "Id") == id, ct);
        }

        public void Update(T entity)
        {
            ArgumentNullException.ThrowIfNull(entity);
            _context.Set<T>().Update(entity);
        }
    }
}
