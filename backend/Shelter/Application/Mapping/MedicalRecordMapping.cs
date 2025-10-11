using Application.Dtos.Animal;
using Application.Dtos.MedicalRecord;
using Domain.Entities;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Mapping
{
    public static class MedicalRecordMapping
    {
        // Entity -> DTO (READ)
        public static MedicalRecordDto MedicalRecordToDto(MedicalRecord medicalRecord)
        {
            return new MedicalRecordDto
            {
                Id = medicalRecord.Id,
                AnimalId = medicalRecord.AnimalId,
                Type = medicalRecord.Type,
                Date = medicalRecord.Date,
                NextDueDate = medicalRecord.NextDueDate,
                Vet = medicalRecord.Vet ?? String.Empty,
                Notes = medicalRecord.Notes ?? String.Empty
            };
        }

        // Create DTO -> Entity (CREATE)
        public static MedicalRecord FromCreateDto(CreateMedicalRecordDto dto) => new MedicalRecord
        {
            AnimalId = dto.AnimalId,
            Type = dto.Type,
            Date = dto.Date,
            NextDueDate = NormalizeNextDue(dto.Type, dto.NextDueDate),
            Vet = dto.Vet ?? string.Empty,
            Notes = dto.Notes ?? string.Empty
        };

        private static DateOnly? NormalizeNextDue(MedicalRecordType type, DateOnly? nextDue)
            => type == MedicalRecordType.Sterilization ? null : nextDue;
    }
}
