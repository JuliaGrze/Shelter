using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos.Adoptions
{
    public class GenerateContractResponse
    {
        public int ContractId { get; set; }
        public string PdfUrl { get; set; } = default!;
        public string PdfHash { get; set; } = default!;
        public string VerificationQrContent { get; set; } = default!;
        public DateTime GeneratedAtUtc { get; set; }
    }
}
