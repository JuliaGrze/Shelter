using Application.Common;
using Application.Dtos;
using Application.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IAnimalService
    {
        Task<List<AnimalDto>> GetAnimalsAsync(CancellationToken ct = default);
        Task<AnimalDto> GetAnimalByIdAsync(int id, CancellationToken ct = default);
        Task<int> AddAnimalAsync(CreateAnimalDto animal, CancellationToken ct = default);
        Task<bool> UpdateAnimalAsync(int id, CreateAnimalDto animal, CancellationToken ct = default);
        Task<bool> DeleteAsync(int id, CancellationToken ct = default);


        /// <summary>
        /// Returns a paged, sorted and filtered list of animals.
        /// All filtering/sorting happens on IQueryable (database-side) for performance.
        /// </summary>
        Task<PagedResult<AnimalDto>> GetAnimalsAsync(AnimalQuery q, CancellationToken ct = default);
    }
}
