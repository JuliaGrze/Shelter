using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    /// <summary>
    /// Będzie reprezentować wizytę domową przypisaną do konkretnego wniosku adopcyjnego (AdoptionApplication)
    /// </summary>
    public class HomeVisit
    {
        public int Id { get; set; }

        public int AdoptionApplicationId { get; set; }
        public AdoptionApplication AdoptionApplication { get; set; } = default!;

        // Data i wynik wizyty
        public DateTime Date { get; set; }         // kiedy ma się odbyć

        public int HomeVisitResultId { get; set; }
        public HomeVisitResult HomeVisitResult { get; set; } = default!;        // np. "Passed", "Failed", "Pending"
        public string? Notes { get; set; }         // dodatkowe uwagi pracownika

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
