using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos.Adoptions
{
    public class AdoptionDetailsDto
    {
        public int Id { get; set; }

        public int AnimalId { get; set; }
        public string AnimalName { get; set; } = default!;
        public string AnimalSpecies { get; set; } = default!;
        public string? AnimalPhotoUrl { get; set; }

        public string ApplicantUserId { get; set; } = default!;
        public string ApplicantEmail { get; set; } = default!;

        public string StatusCode { get; set; } = default!;
        public string StatusName { get; set; } = default!;
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }

        public HomeVisitBlock? HomeVisit { get; set; }
        public ContractBlock? Contract { get; set; }

        public record HomeVisitBlock(DateTime? Date, string? ResultCode, string? Notes);
        public record ContractBlock(bool Generated, bool Signed, string? FileUrl);
    }
}

