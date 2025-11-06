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
    public class AnimalConfiguration : IEntityTypeConfiguration<Animal>
    {
        public void Configure(EntityTypeBuilder<Animal> e)
        {
            e.Property(x => x.Name).HasMaxLength(100).IsRequired();
            e.Property(x => x.Description).HasMaxLength(2000);

            //Animal sex as a string in DB
            e.Property(x => x.Sex)
                .HasConversion<string>()
                .HasMaxLength(10).IsRequired();
            e.Property(x => x.Status)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            // Relacja: Animal → Species (nie kasuj gatunku przy usuwaniu zwierzaka)    
            e.HasOne(a => a.Species)
                .WithMany(s => s.Animals)
                .HasForeignKey(a => a.SpeciesId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relacja: Animal → MedicalRecord (usuwa rekordy medyczne wraz z Animal)
            e.HasMany(a => a.MedicalRecords)
                .WithOne(m => m.Animal)
                .HasForeignKey(m => m.AnimalId)
                .OnDelete(DeleteBehavior.Cascade);

        }
    }
}
