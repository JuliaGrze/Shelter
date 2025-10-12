using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos.MedicalRecord
{
    public  class DueMedicalRecordDto
    {
        public int Id { get; set; }
        public int AnimalId { get; set; }
        public string AnimalName { get; set; } = String.Empty;
        public MedicalRecordType Type { get; set; }
        public DateOnly Date { get; set; }
        public DateOnly? NextDueDate { get; set; }

        // How many days until the due date (negative = overdue)
        public int? DaysUntilDue { get; set; }

        // Has the deadline passed yet? (NextDueDate < today)
        public bool Overdue { get; set; }
    }
}
