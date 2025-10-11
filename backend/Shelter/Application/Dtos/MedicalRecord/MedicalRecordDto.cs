using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos.MedicalRecord
{
    public class MedicalRecordDto
    {
        public int Id { get; set; }
        public int AnimalId { get; set; }
        public MedicalRecordType Type { get; set; }
        public DateOnly Date { get; set; }
        public DateOnly? NextDueDate { get; set; }
        public string? Vet { get; set; }
        public string? Notes { get; set; }
    }
}
