using Application.Common;
using Application.Dtos;
using Application.Interfaces;
using Application.Mapping;
using Application.Queries;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;

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

        // returns paged & sorted animals
        public async Task<PagedResult<AnimalDto>> GetAnimalsAsync(AnimalQuery q, CancellationToken ct = default)
        {
            // Base query with species include
            var baseQuery = _repository.Query(a => a.Species);

            // Apply sorting (based on AnimalSortField + SortDirection)
            var sorted = (q.SortBy, q.SortDir) switch
            {
                (AnimalSortField.name, SortDirection.asc) => baseQuery.OrderBy(a => a.Name),
                (AnimalSortField.name, SortDirection.desc) => baseQuery.OrderByDescending(a => a.Name),

                (AnimalSortField.species, SortDirection.asc) => baseQuery.OrderBy(a => a.Species),
                (AnimalSortField.species, SortDirection.desc) => baseQuery.OrderByDescending(a => a.Species),

                (AnimalSortField.birthDate, SortDirection.asc) => baseQuery.OrderBy(a => a.BirthDate),
                (AnimalSortField.birthDate, SortDirection.desc) => baseQuery.OrderByDescending(a => a.BirthDate),

                (AnimalSortField.sex, SortDirection.asc) => baseQuery.OrderBy(a => a.Sex),
                (AnimalSortField.sex, SortDirection.desc) => baseQuery.OrderByDescending(a => a.Sex),

                (AnimalSortField.status, SortDirection.asc) => baseQuery.OrderBy(a => a.Status),
                (AnimalSortField.status, SortDirection.desc) => baseQuery.OrderByDescending(a => a.Status),

                (AnimalSortField.createdAt, SortDirection.asc) => baseQuery.OrderBy(a => a.CreatedAt),
                (AnimalSortField.createdAt, SortDirection.desc) => baseQuery.OrderByDescending(a => a.CreatedAt),

                _ => baseQuery.OrderByDescending(a => a.CreatedAt)
            };

            //Pagination
            var total = await sorted.CountAsync();
            var items = sorted
                .Skip(q.Page * q.Size)
                .Take(q.Size)
                .Select(AnimalMapping.AnimalToDto)
                .ToList();

            // Return paginated result
            return new PagedResult<AnimalDto>(items, total, q.Page, q.Size);

        }
    }
}
