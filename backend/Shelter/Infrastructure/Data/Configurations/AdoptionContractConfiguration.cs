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
    public class AdoptionContractConfiguration : IEntityTypeConfiguration<AdoptionContract>
    {
        public void Configure(EntityTypeBuilder<AdoptionContract> b)
        {
            b.ToTable("AdoptionContracts");
            b.HasKey(x => x.Id);

            // Relacja 1–1 z AdoptionApplication
            b.HasOne(x => x.AdoptionApplication)
             .WithOne(a => a.Contract)
             .HasForeignKey<AdoptionContract>(x => x.AdoptionApplicationId)
             .OnDelete(DeleteBehavior.Cascade);

            // Wymagane pola pliku i weryfikacji
            b.Property(x => x.PdfUrl).HasMaxLength(500).IsRequired();
            b.Property(x => x.PdfHash).HasMaxLength(128).IsRequired();              // HEX SHA-256 mieści się swobodnie
            b.Property(x => x.VerificationQrContent).HasMaxLength(1000).IsRequired();

            // Indeksy pomocnicze
            b.HasIndex(x => x.PdfHash).IsUnique();                                  // szybka weryfikacja po hash
            b.HasIndex(x => x.AdoptionApplicationId).IsUnique();                    // 1–1

            // Domyślne wartości
            b.Property(x => x.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            b.Property(x => x.Notes).HasMaxLength(2000);
            b.Property(x => x.SignedByUserId).HasMaxLength(450); // zgodnie z rozmiarem klucza Identity
        }
    }
}
