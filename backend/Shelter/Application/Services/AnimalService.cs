using Application.Common;
using Application.Dtos.Animal;
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

            entity.Status = dto.Status.ToString();
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

        // returns paged, sorted & filtered animals
        public async Task<PagedResult<AnimalDto>> GetAnimalsAsync(AnimalQuery q, CancellationToken ct = default)
        {
            // Base query with required include for Species (we need Species.Name in DTO)
            // _repository.Query(a => a.Species) internally does DbSet.Include(a => a.Species)
            var query = _repository.Query(a => a.Species);

            // ------------------- FILTERS -------------------

            // Species filter
            if (q.SpeciesId.HasValue)
                query = query.Where(a => a.SpeciesId == q.SpeciesId.Value);

            // Sex filter (entity stores string; query uses enum -> string)
            if (q.Sex.HasValue)
            {
                var sexText = q.Sex.Value.ToString(); // "Male" | "Female" | "Unknown"
                query = query.Where(a => a.Sex == sexText);
            }

            // Status filter (enum -> string)
            if (q.Status.HasValue)
            {
                var statusText = q.Status.Value.ToString(); // "Available" | "Reserved" | ...
                query = query.Where(a => a.Status == statusText);
            }

            // Vaccinated / Neutered flags
            if (q.Vaccinated.HasValue)
                query = query.Where(a => a.Vaccinated == q.Vaccinated.Value);

            if (q.Neutered.HasValue)
                query = query.Where(a => a.Neutered == q.Neutered.Value);

            // Age (in months) -> convert to BirthDate range using DateOnly
            var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);

            // Minimum age: animal must be at least X months old => BirthDate <= today - X months
            if (q.AgeMinMonths.HasValue)
            {
                var cutoffOlder = today.AddMonths(-q.AgeMinMonths.Value);
                query = query.Where(a => a.BirthDate <= cutoffOlder);
            }

            // Maximum age: animal must be at most Y months old => BirthDate >= today - Y months
            if (q.AgeMaxMonths.HasValue)
            {
                var cutoffYounger = today.AddMonths(-q.AgeMaxMonths.Value);
                query = query.Where(a => a.BirthDate >= cutoffYounger);
            }

            // CreatedAt range (inclusive)
            if (q.CreatedFrom.HasValue)
                query = query.Where(a => a.CreatedAt >= q.CreatedFrom.Value);

            if (q.CreatedTo.HasValue)
            {
                // If you want full-day inclusive behavior, you can expand 'to' to the end of day:
                // var inclusiveTo = q.CreatedTo.Value.Date.AddDays(1).AddTicks(-1);
                // query = query.Where(a => a.CreatedAt <= inclusiveTo);
                query = query.Where(a => a.CreatedAt <= q.CreatedTo.Value);
            }

            // ------------------- SORTING -------------------
            // IMPORTANT: species sorting must use a.Species.Name (not a.Species)
            var sorted = (q.SortBy, q.SortDir) switch
            {
                (AnimalSortField.name, SortDirection.asc) => query.OrderBy(a => a.Name),
                (AnimalSortField.name, SortDirection.desc) => query.OrderByDescending(a => a.Name),

                (AnimalSortField.species, SortDirection.asc) => query.OrderBy(a => a.Species.Name).ThenBy(a => a.Name),
                (AnimalSortField.species, SortDirection.desc) => query.OrderByDescending(a => a.Species.Name).ThenBy(a => a.Name),

                (AnimalSortField.birthDate, SortDirection.asc) => query.OrderBy(a => a.BirthDate).ThenBy(a => a.Name),
                (AnimalSortField.birthDate, SortDirection.desc) => query.OrderByDescending(a => a.BirthDate).ThenBy(a => a.Name),

                (AnimalSortField.sex, SortDirection.asc) => query.OrderBy(a => a.Sex).ThenBy(a => a.Name),
                (AnimalSortField.sex, SortDirection.desc) => query.OrderByDescending(a => a.Sex).ThenBy(a => a.Name),

                (AnimalSortField.status, SortDirection.asc) => query.OrderBy(a => a.Status).ThenBy(a => a.Name),
                (AnimalSortField.status, SortDirection.desc) => query.OrderByDescending(a => a.Status).ThenBy(a => a.Name),

                (AnimalSortField.createdAt, SortDirection.asc) => query.OrderBy(a => a.CreatedAt),
                (AnimalSortField.createdAt, SortDirection.desc) => query.OrderByDescending(a => a.CreatedAt),

                _ => query.OrderByDescending(a => a.CreatedAt)
            };

            // ------------------- PAGING + PROJECTION -------------------
            var total = await sorted.CountAsync(ct);

            var items = sorted
                .Skip(q.Page * q.Size)
                .Take(q.Size)
                .Select(AnimalMapping.AnimalToDto) // Project entity -> DTO on the DB side
                .ToList();

            return new PagedResult<AnimalDto>(items, total, q.Page, q.Size);
        }

    }
}
