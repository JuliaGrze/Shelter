using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos.Adoptions
{
    public class AdoptionListItemDto
    {
        public int Id { get; set; }
        public int AnimalId { get; set; }
        public string AnimalName { get; set; } = default!;
        public string AnimalSpecies { get; set; } = default!;
        public string? AnimalPhotoUrl { get; set; }

        public string ApplicantEmail { get; set; } = default!;
        public string StatusCode { get; set; } = default!;
        public DateTime CreatedAt { get; set; }
    }
}
