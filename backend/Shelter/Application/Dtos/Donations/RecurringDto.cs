using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos.Donations
{
    public class RecurringDto
    {
        public string PriceId { get; set; } = default!;
        public string? DonorPublicName { get; set; }
        public bool IsPublic { get; set; }
        public string? Message { get; set; }
    }
}
