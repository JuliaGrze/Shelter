using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos.Donations
{
    public class OneTimeDto
    {
        public long AmountMinor { get; set; }
        public string? Currency { get; set; }
        public string? DonorPublicName { get; set; } 
        public bool IsPublic { get; set; }
        public string? Message { get; set; }
    }
}
