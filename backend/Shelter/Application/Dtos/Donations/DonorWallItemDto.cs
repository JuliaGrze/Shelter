using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos.Donations
{
    public class DonorWallItemDto
    {
        public string DonorPublicName { get; set; } = default!;
        public long AmountMinor { get; set; }
        public string Currency { get; set; } = "PLN";
        public DateTime CreatedAt { get; set; }

        public DonorWallItemDto() { }

        public DonorWallItemDto(string donorPublicName, long amountMinor, string currency, DateTime createdAt)
        {
            DonorPublicName = donorPublicName;
            AmountMinor = amountMinor;
            Currency = currency;
            CreatedAt = createdAt;
        }
    }
}
