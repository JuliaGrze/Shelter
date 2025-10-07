using Application.Dtos;
using Application.Interfaces;
using Application.Mapping;
using Domain.Entities;
using Infrastructure.Repositories.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class SpeciesService : ISpeciesService
    {
        private readonly IGenericRepository<Species> _speciesRepository;
        public SpeciesService(IGenericRepository<Species> speciesRepository)
        {
            _speciesRepository = speciesRepository;
        }

        public async Task<List<SpeciesDto>> GetSpeciesAsync(CancellationToken ct = default)
        {
            var species = await _speciesRepository.GetAllAsync(ct);
            return species.Select(SpeciesMapping.SpeciestoDto).ToList();
        }

        public Task<SpeciesDto> GetSpecieByIdAsync(int id, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<int> AddSpecieAsync(CreateAnimalDto animal, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UpdateSpecieAsync(int id, CreateAnimalDto animal, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteSpecieAsync(int id, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        
    }
}
