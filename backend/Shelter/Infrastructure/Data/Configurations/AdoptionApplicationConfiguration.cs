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
    public class AdoptionApplicationConfiguration : IEntityTypeConfiguration<AdoptionApplication>
    {
        public void Configure(EntityTypeBuilder<AdoptionApplication> b)
        {
            // Tabela
            b.ToTable("AdoptionApplications");

            // AdoptionApplication (wiele) -> Animal (jeden)
            b.HasOne(x => x.Animal)
             .WithMany(a => a.AdoptionApplications)
             .HasForeignKey(x => x.AnimalId)       
             .OnDelete(DeleteBehavior.Restrict);

            // AdoptionApplication (wiele) -> ApplicationUser (jeden)
            b.HasOne(x => x.ApplicationUser)
             .WithMany(u => u.AdoptionApplications) 
             .HasForeignKey(x => x.ApplicationUserId)
             .OnDelete(DeleteBehavior.Restrict);

            // AdoptionApplication (wiele) -> AdoptionStatus (jeden)
            b.HasOne(x => x.AdoptionStatus)
             .WithMany()            
             .HasForeignKey(x => x.AdoptionStatusId)
             .OnDelete(DeleteBehavior.Restrict);

            // Pola
            b.Property(x => x.ApplicationUserId)
             .HasMaxLength(450)     
             .IsRequired();

            b.Property(x => x.Notes)
             .HasMaxLength(2000);

            b.Property(x => x.CreatedAt)
             .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // 1-1 HomeVisit
            b.HasOne(x => x.HomeVisit)
             .WithOne(h => h.AdoptionApplication)
             .HasForeignKey<HomeVisit>(h => h.AdoptionApplicationId)
             .OnDelete(DeleteBehavior.Cascade);

            // 1-1 Contract
            b.HasOne(x => x.Contract)
             .WithOne(c => c.AdoptionApplication)
             .HasForeignKey<AdoptionContract>(c => c.AdoptionApplicationId)
             .OnDelete(DeleteBehavior.Cascade);

            // Indeksy pomocnicze
            b.HasIndex(x => x.AnimalId);
            b.HasIndex(x => x.ApplicationUserId);
            b.HasIndex(x => x.AdoptionStatusId);
            b.HasIndex(x => x.CreatedAt);
        }
    }
}
