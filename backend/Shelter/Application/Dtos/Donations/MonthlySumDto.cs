using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos.Donations
{
    public class MonthlySumDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string Currency { get; set; } = "PLN";
        public long AmountMinor { get; set; }
        public int Count { get; set; }

        public MonthlySumDto() { }

        public MonthlySumDto(int year, int month, string currency, long amountMinor, int count)
        {
            Year = year;
            Month = month;
            Currency = currency;
            AmountMinor = amountMinor;
            Count = count;
        }
    }
}
