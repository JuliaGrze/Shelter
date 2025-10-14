using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Data.Configurations
{
    public class DonationConfiguration : IEntityTypeConfiguration<Donation>
    {
        public void Configure(EntityTypeBuilder<Donation> b)
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.AmountMinor).IsRequired();
            b.Property(x => x.Currency).HasMaxLength(10).IsRequired();
            b.Property(x => x.DonorPublicName).HasMaxLength(100);
            b.Property(x => x.StripePaymentIntentId).HasMaxLength(200).IsRequired();
            b.Property(x => x.StripeCustomerId).HasMaxLength(200);
            b.Property(x => x.Message).HasMaxLength(500);
            // GETUTCDATE() - To funkcja SQL Server, która zwraca aktualny czas UTC
            b.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            b.HasIndex(x => x.CreatedAt);
            b.HasIndex(x => new { x.IsRecurring, x.CreatedAt });
        }
    }
}
