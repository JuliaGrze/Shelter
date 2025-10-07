using Application.Dtos;
using Application.Interfaces;
using Application.Mapping;
using Domain.Entities;
using Infrastructure.Repositories.Abstractions;

namespace Application.Services
{
    public class SpeciesService : ISpeciesService
    {
        private readonly IGenericRepository<Species> _speciesRepository;
        private readonly IUnitOfWork _unitOfWork;

        public SpeciesService(IGenericRepository<Species> speciesRepository, IUnitOfWork unitOfWork)
        {
            _speciesRepository = speciesRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<List<SpeciesDto>> GetSpeciesAsync(CancellationToken ct = default)
        {
            var species = await _speciesRepository.GetAllAsync(ct);
            return species.Select(SpeciesMapping.SpeciestoDto).ToList();
        }

        public async Task<SpeciesDto> GetSpecieByIdAsync(int id, CancellationToken ct = default)
        {
            var entity = await _speciesRepository.GetByIdAsync(id, ct);
            if (entity is null)
                throw new KeyNotFoundException($"Species with id={id} not found.");

            return SpeciesMapping.SpeciestoDto(entity);
        }

        public async Task<int> AddSpecieAsync(CreateSpeciesDto speciesDto, CancellationToken ct = default)
        {
            var entity = SpeciesMapping.DtoToSpecies(speciesDto);
            entity.Name = entity.Name?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(entity.Name))
                throw new ArgumentException("Species name cannot be empty.", nameof(speciesDto));

            //(optional) check for duplicate name:
            if (await _speciesRepository.AnyAsync(s => s.Name == entity.Name.Trim(), ct))
                throw new InvalidOperationException($"Species '{entity.Name}' already exists.");

            await _speciesRepository.AddAsync(entity, ct);
            await _unitOfWork.SaveChangesAsync(ct);
            return entity.Id; // EF ustawi Id po SaveChangesAsync
        }

        public async Task<bool> UpdateSpecieAsync(int id, CreateSpeciesDto speciesDto, CancellationToken ct = default)
        {
            var entity = await _speciesRepository.GetByIdAsync(id, ct);
            if (entity is null)
                throw new KeyNotFoundException($"Species with id={id} not found.");

            var newName = (speciesDto.Name ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(newName))
                throw new ArgumentException("Species name cannot be empty.", nameof(speciesDto));

            //(optional) check for duplicate name:
            if (await _speciesRepository.AnyAsync(s => s.Id != id && s.Name == newName.Trim(), ct))
                throw new InvalidOperationException($"Species '{newName}' already exists.");

            entity.Name = newName.Trim();

            _speciesRepository.Update(entity);           
            await _unitOfWork.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> DeleteSpecieAsync(int id, CancellationToken ct = default)
        {
            var exist = await _speciesRepository.AnyAsync(a => a.Id == id);
            if (exist == false) return false;

            _speciesRepository.Delete(id);     
            await _unitOfWork.SaveChangesAsync(ct);
            return true;
        }
    }
}
