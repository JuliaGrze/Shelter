using Application.Dtos.Species;
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
            Validate(speciesDto);

            var name = (speciesDto.Name ?? string.Empty).Trim();
            if (await _speciesRepository.AnyAsync(s => s.Name.ToLower() == name.ToLower(), ct))
                throw new InvalidOperationException($"Species '{name}' already exists.");

            var entity = SpeciesMapping.DtoToSpecies(speciesDto);

            await _speciesRepository.AddAsync(entity, ct);
            await _unitOfWork.SaveChangesAsync(ct);
            return entity.Id;
        }

        public async Task<bool> UpdateSpecieAsync(int id, CreateSpeciesDto speciesDto, CancellationToken ct = default)
        {
            Validate(speciesDto);

            var entity = await _speciesRepository.GetByIdAsync(id, ct);
            if (entity is null)
                throw new KeyNotFoundException($"Species with id={id} not found.");

            var newName = (speciesDto.Name ?? string.Empty).Trim();
            if (await _speciesRepository.AnyAsync(s => s.Id != id && s.Name.ToLower() == newName.ToLower(), ct))
                throw new InvalidOperationException($"Species '{newName}' already exists.");

            // aktualizacja wszystkich pól
            entity.Name = newName;
            entity.RequiresPermit = speciesDto.RequiresPermit;
            entity.PermitName = string.IsNullOrWhiteSpace(speciesDto.PermitName) ? null : speciesDto.PermitName.Trim();
            entity.PermitAuthority = string.IsNullOrWhiteSpace(speciesDto.PermitAuthority) ? null : speciesDto.PermitAuthority.Trim();
            entity.PermitNotes = string.IsNullOrWhiteSpace(speciesDto.PermitNotes) ? null : speciesDto.PermitNotes.Trim();

            _speciesRepository.Update(entity);
            await _unitOfWork.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> DeleteSpecieAsync(int id, CancellationToken ct = default)
        {
            var exist = await _speciesRepository.AnyAsync(a => a.Id == id, ct);
            if (!exist) return false;

            _speciesRepository.Delete(id);
            await _unitOfWork.SaveChangesAsync(ct);
            return true;
        }

        private static void Validate(CreateSpeciesDto dto)
        {
            var name = (dto.Name ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Species name cannot be empty.", nameof(dto));

            if (dto.RequiresPermit)
            {
                if (string.IsNullOrWhiteSpace(dto.PermitName))
                    throw new ArgumentException("PermitName is required when RequiresPermit is true.", nameof(dto));
                if (string.IsNullOrWhiteSpace(dto.PermitAuthority))
                    throw new ArgumentException("PermitAuthority is required when RequiresPermit is true.", nameof(dto));
            }
        }
    }
}
