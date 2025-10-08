using Application.Dtos.Species;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface ISpeciesService
    {
        Task<List<SpeciesDto>> GetSpeciesAsync(CancellationToken ct = default);
        Task<SpeciesDto> GetSpecieByIdAsync(int id, CancellationToken ct = default);
        Task<int> AddSpecieAsync(CreateSpeciesDto animal, CancellationToken ct = default);
        Task<bool> UpdateSpecieAsync(int id, CreateSpeciesDto animal, CancellationToken ct = default);
        Task<bool> DeleteSpecieAsync(int id, CancellationToken ct = default);
    }
}
