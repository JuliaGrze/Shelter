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
    public class HomeVisitResultConfiguration : IEntityTypeConfiguration<HomeVisitResult>
    {
        public void Configure(EntityTypeBuilder<HomeVisitResult> b)
        {
            b.ToTable("HomeVisitResults");

            b.Property(x => x.Code).HasMaxLength(40).IsRequired();
            b.Property(x => x.Name).HasMaxLength(100).IsRequired();
            b.HasIndex(x => x.Code).IsUnique();

            // SEED — bazowy słownik wyników wizyt
            b.HasData(
                new HomeVisitResult { Id = 1, Code = "Pending", Name = "W trakcie / oczekuje", IsFinal = false },
                new HomeVisitResult { Id = 2, Code = "Passed", Name = "Pozytywny", IsFinal = true },
                new HomeVisitResult { Id = 3, Code = "Failed", Name = "Negatywny", IsFinal = true },
                new HomeVisitResult { Id = 4, Code = "Cancelled", Name = "Odwołana", IsFinal = true },
                new HomeVisitResult { Id = 5, Code = "Rescheduled", Name = "Przełożona", IsFinal = false }
            );
        }
    }
}
