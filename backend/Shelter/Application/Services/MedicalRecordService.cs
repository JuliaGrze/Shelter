using Application.Dtos.MedicalRecord;
using Application.Interfaces;
using Application.Mapping;
using Domain.Entities;
using Infrastructure.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class MedicalRecordService : IMedicalRecordService
    {
        private readonly IGenericRepository<MedicalRecord> _medicalRecordRepository;
        private readonly IGenericRepository<Animal> _animalRepository;
        private readonly IUnitOfWork _unitOfWork;
        public MedicalRecordService(IGenericRepository<MedicalRecord> genericRepository,
            IGenericRepository<Animal> animalRepository,
            IUnitOfWork unitOfWork)
        {
            _medicalRecordRepository = genericRepository;
            _animalRepository = animalRepository;
            _unitOfWork = unitOfWork;
        }

        // CREATE from CreateMedicalRecordDto
        public async Task<int> CreateMedicalRecordAsync(CreateMedicalRecordDto medicalRecordDto, CancellationToken ct = default)
        {
            //validation
            var animalExist = await _animalRepository.AnyAsync(a => a.Id == medicalRecordDto.AnimalId);
            if (!animalExist)
                throw new KeyNotFoundException($"Animal #{medicalRecordDto.AnimalId} not found.");

            var entity = MedicalRecordMapping.FromCreateDto(medicalRecordDto);

            await _medicalRecordRepository.AddAsync(entity, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            return entity.Id;
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
        {
            var exists = await _medicalRecordRepository.AnyAsync(m => m.Id == id, ct);
            if (!exists) return false;

            _medicalRecordRepository.Delete(id);
            await _unitOfWork.SaveChangesAsync(ct);
            return true;
        }


        // GET all records for a given animal (sorted by Date desc)
        public async Task<List<MedicalRecordDto>> GetMedicalRecordByAnimalAsync(int animalId, CancellationToken ct = default)
        {
            var query = _medicalRecordRepository
                .Query()
                .Where(r => r.AnimalId == animalId)
                .OrderByDescending(r => r.Date)
                .Select(r => MedicalRecordMapping.MedicalRecordToDto(r));

            // ToListAsync(ct) wykonuje zapytanie SQL w bazie danych.
            // Do tej pory wszystko było tylko “planem zapytania” (LINQ → SQL)
            return await query.ToListAsync(ct);
        }

        public async Task<bool> UpdateAsync(int id, CreateMedicalRecordDto medicalRecordDto, CancellationToken ct = default)
        {
            var entity = await _medicalRecordRepository.GetByIdAsync(id);
            if(entity is null) return false;

            //we dont allow to change animalId in existing record
            if (entity.AnimalId != medicalRecordDto.AnimalId)
                throw new InvalidOperationException("Changing AnimalId of a medical record is not allowed.");

            if (entity.Type != medicalRecordDto.Type)
                throw new InvalidOperationException("Changing Type of a medical record is not allowed.");

            entity.Date = medicalRecordDto.Date;
            entity.NextDueDate = (medicalRecordDto.Type == Domain.Enums.MedicalRecordType.Sterilization)
                ? null
                : medicalRecordDto.NextDueDate;
            entity.Vet = medicalRecordDto.Vet ?? string.Empty;
            entity.Notes = medicalRecordDto.Notes ?? string.Empty;

            _medicalRecordRepository.Update(entity);
            await _unitOfWork.SaveChangesAsync(ct);
            return true;

        }


        public async Task<List<DueMedicalRecordDto>> GetDueWithinDaysAsync(int days, CancellationToken ct = default)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow); //np. 10.10.2025
            var threshold = today.AddDays(days); //np. 17.10.2025

            //var query = _medicalRecordRepository
            //    .Query(x => x.Animal)
            //    .AsNoTracking()
            //    .Where(x => x.NextDueDate != null && x.NextDueDate < trashhold)
            //    .OrderBy(x => x.NextDueDate);

            //var records = await query.ToListAsync(ct);

            // Kandydaci: mają NextDueDate i termin ≤ threshold
            // Drugie sito: NIE istnieje nowszy wpis tego samego (AnimalId, Type)
            // (nowszy = większe Date, a przy remisie po Date -> większe Id)
            var latestPerAnimalTypeWithinThreshold = await _medicalRecordRepository
               .Query(x => x.Animal)
               .AsNoTracking()
               .Where(x => x.NextDueDate != null && x.NextDueDate <= threshold)
               .Where(x => !_medicalRecordRepository.Query()
                   .Any(y =>
                       y.AnimalId == x.AnimalId &&
                       y.Type == x.Type &&
                       (
                           y.NextDueDate > x.NextDueDate ||
                           (y.NextDueDate == x.NextDueDate && y.Id > x.Id)
                       )
                   ))
               .OrderBy(x => x.NextDueDate)
               .ToListAsync(ct);

            // Convert to DTO using existing mapper
            var result = latestPerAnimalTypeWithinThreshold.
                Select(r => MedicalRecordMapping.ToDueMedicalRecordDto(r.Animal.Name, r, today))
                         .ToList();

            return result;
        }
    }
}
