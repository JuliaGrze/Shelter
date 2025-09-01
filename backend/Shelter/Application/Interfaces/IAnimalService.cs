using Application.Dtos;
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

    }
}
