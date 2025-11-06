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
    public class AdoptionStatusConfiguration : IEntityTypeConfiguration<AdoptionStatus>
    {
        public void Configure(EntityTypeBuilder<AdoptionStatus> b)
        {
            b.Property(x => x.Code).HasMaxLength(40).IsRequired();
            b.Property(x => x.Name).HasMaxLength(100).IsRequired();

            b.HasIndex(x => x.Code).IsUnique();

            // SEED — stałe ID
            b.HasData(
               new AdoptionStatus { Id = 1, Code = "Submitted", Name = "Złożony wniosek", IsFinal = false },
               new AdoptionStatus { Id = 2, Code = "InReview", Name = "W trakcie weryfikacji", IsFinal = false },
               new AdoptionStatus { Id = 3, Code = "HomeVisitScheduled", Name = "Wizyta domowa umówiona", IsFinal = false },
               new AdoptionStatus { Id = 4, Code = "HomeVisitCompleted", Name = "Wizyta domowa odbyta", IsFinal = false },
               new AdoptionStatus { Id = 5, Code = "Approved", Name = "Zatwierdzony", IsFinal = false },
               new AdoptionStatus { Id = 6, Code = "Rejected", Name = "Odrzucony", IsFinal = true },
               new AdoptionStatus { Id = 7, Code = "Withdrawn", Name = "Wycofany przez wnioskodawcę", IsFinal = true },
               new AdoptionStatus { Id = 8, Code = "ContractSigned", Name = "Umowa podpisana", IsFinal = true },
               new AdoptionStatus { Id = 9, Code = "ContractGenerated", Name = "Umowa wygenerowana", IsFinal = false }
           );
        }
    }
}
