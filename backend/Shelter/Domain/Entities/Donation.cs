using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Donation
    {
        public int Id { get; set; }
        public long AmountMinor { get; set; } //grosze
        public string Currency { get; set; } = "PLN";
        public string? DonorPublicName { get; set; }
        public bool IsRecurring { get; set; } //powtarza sie
        public string StripePaymentIntentId { get; set; } = default!; // lub Invoice/Subscription dla cyklicznych
        public string? StripeCustomerId { get; set; }
        public bool IsPublic { get; set; } = true;
        public string? Message { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
