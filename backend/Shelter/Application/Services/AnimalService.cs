using Application.Dtos;
using Application.Interfaces;
using Application.Mapping;
using Domain.Entities;
using Infrastructure.Repositories.Abstractions;

namespace Application.Services
{
    public class AnimalService : IAnimalService
    {
        private readonly IGenericRepository<Animal> _repository;
        private readonly IUnitOfWork _unitOfWork;

        public AnimalService(IGenericRepository<Animal> genericRepository, IUnitOfWork unitOfWork)
        {
            _repository = genericRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<int> AddAnimalAsync(CreateAnimalDto dto, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new ArgumentException("Name cannot be empty.", nameof(dto.Name));
            if (dto.SpeciesId <= 0)
                throw new ArgumentException("SpeciesId must be positive.", nameof(dto.SpeciesId));

            var entity = AnimalMapping.DtoToAnimal(dto);      // DTO -> Entity
            await _repository.AddAsync(entity, ct);
            await _unitOfWork.SaveChangesAsync(ct);           // DB nada Id
            return entity.Id;                                 // ⬅️ zwracamy Id
        }

        public async Task<List<AnimalDto>> GetAnimalsAsync(CancellationToken ct = default)
        {
            // Dołączamy Species, żeby SpeciesName w DTO nie było null
            var animals = await _repository.GetAllAsync(ct, a => a.Species);
            return animals.Select(AnimalMapping.AnimalToDto).ToList();
        }

        public async Task<AnimalDto> GetAnimalByIdAsync(int id, CancellationToken ct = default)
        {
            var entity = await _repository.GetByIdAsync(id, ct, a => a.Species);
            if (entity is null)
                throw new KeyNotFoundException($"Animal with id={id} not found.");

            return AnimalMapping.AnimalToDto(entity);
        }

        public async Task<bool> UpdateAnimalAsync(int id, CreateAnimalDto dto, CancellationToken ct = default)
        {
            var entity = await _repository.GetByIdAsync(id, ct); 
            if (entity is null) return false;

            entity.Name = dto.Name.Trim();
            entity.SpeciesId = dto.SpeciesId;
            entity.BirthDate = dto.BirthDate;
            entity.Sex = dto.Sex.ToString();                // jeśli w encji string; gdy enum → entity.Sex = dto.Sex;

            entity.Status   = dto.Status.ToString();
            entity.Description = dto.Description?.Trim() ?? "";
            entity.Vaccinated = dto.Vaccinated;
            entity.Neutered = dto.Neutered;
            entity.PhotoUrl = string.IsNullOrWhiteSpace(dto.PhotoUrl) ? null : dto.PhotoUrl.Trim();

            _repository.Update(entity);
            await _unitOfWork.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
        {
            var entity = await _repository.GetByIdAsync(id, ct);
            if (entity is null) return false;

            _repository.Delete(id);
            await _unitOfWork.SaveChangesAsync(ct);   
            return true;
        }
    }
}
