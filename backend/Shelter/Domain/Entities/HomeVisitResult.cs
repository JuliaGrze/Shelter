using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class HomeVisitResult
    {
        public int Id { get; set; }
        public string Code { get; set; } = String.Empty; // "Pending", "Passed", "Failed", "Cancelled", "Rescheduled"
        public string Name { get; set; } = String.Empty; // np. "W trakcie", "Pozytywny", "Negatywny", ...
        public string? Description { get; set; }
        public bool IsFinal { get; set; }     // Passed/Failed/Cancelled jako końcowe
    }
}
