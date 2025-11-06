using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class AdoptionApplication
    {
        public int Id { get; set; }

        //Animal
        public int AnimalId { get; set; }
        public Animal Animal { get; set; } = default!;

        //User
        public string ApplicationUserId { get; set; } = default!;
        public ApplicationUser ApplicationUser { get; set; } = default!;

        //Status
        public int AdoptionStatusId { get; set; }
        public AdoptionStatus AdoptionStatus { get; set; } = default!;

        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public HomeVisit? HomeVisit { get; set; }
        public AdoptionContract? Contract { get; set; }


    }
}
