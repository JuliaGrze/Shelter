using Application.Dtos.MedicalRecord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IMedicalRecordService
    {
        Task<List<MedicalRecordDto>> GetMedicalRecordByAnimalAsync(int animalId, CancellationToken ct = default);
        Task<int> CreateMedicalRecordAsync(CreateMedicalRecordDto medicalRecordDto, CancellationToken ct = default);
        Task<bool> UpdateAsync(int id, CreateMedicalRecordDto medicalRecordDto, CancellationToken ct = default);
        Task<bool> DeleteAsync(int id, CancellationToken ct = default);
    }
}
