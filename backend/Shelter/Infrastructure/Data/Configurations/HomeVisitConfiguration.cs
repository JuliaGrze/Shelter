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
    public class HomeVisitConfiguration : IEntityTypeConfiguration<HomeVisit>
    {
        public void Configure(EntityTypeBuilder<HomeVisit> b)
        {
            b.ToTable("HomeVisits");

            // Relacja 1–1 z AdoptionApplication
            b.HasOne(x => x.AdoptionApplication)
                .WithOne(a => a.HomeVisit)
                .HasForeignKey<HomeVisit>(x => x.AdoptionApplicationId)
                .OnDelete(DeleteBehavior.Cascade);


            // Relacja 1–n z HomeVisitResult
            b.HasOne(x => x.HomeVisitResult)
                .WithMany() // słownik, więc brak odwrotnej nawigacji
                .HasForeignKey(x => x.HomeVisitResultId)
                .OnDelete(DeleteBehavior.Restrict);

            // Ograniczenia pól
            b.Property(x => x.Notes)
                .HasMaxLength(2000);

            b.Property(x => x.Date)
                .IsRequired();

            b.Property(x => x.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
        }
    }
}
