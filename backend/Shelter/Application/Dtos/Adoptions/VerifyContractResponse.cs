using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos.Adoptions
{
    public class VerifyContractResponse
    {
        public bool Exists { get; set; }
        public bool Signed { get; set; }
        public DateTime? SignedAt { get; set; }

        public int ApplicationId { get; set; }
        public int AnimalId { get; set; }
        public string? AnimalName { get; set; }
        public string? ApplicantEmail { get; set; }
    }
}
