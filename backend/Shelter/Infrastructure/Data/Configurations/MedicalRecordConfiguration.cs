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
    public class MedicalRecordConfiguration : IEntityTypeConfiguration<MedicalRecord>
    {
        public void Configure(EntityTypeBuilder<MedicalRecord> builder)
        {
            builder.HasKey(x => x.Id);

            // Enum jako string w DB (czytelny)
            builder.Property(x => x.Type)
                .HasConversion<string>()
                .IsRequired();

            // DateOnly konwersje
            builder.Property(x => x.Date)
             .HasConversion(
                d => d.ToDateTime(TimeOnly.MinValue),
                dt => DateOnly.FromDateTime(dt))
             .IsRequired();

            builder.Property(x => x.NextDueDate)
             .HasConversion(
                d => d.HasValue ? d.Value.ToDateTime(TimeOnly.MinValue) : (DateTime?)null,
                dt => dt.HasValue ? DateOnly.FromDateTime(dt.Value) : (DateOnly?)null);

            builder.Property(x => x.Vet)
             .HasMaxLength(200);

            builder.Property(x => x.Notes)
             .HasMaxLength(1000);

            // Relacja do Animal
            builder.HasOne(x => x.Animal)
             .WithMany(a => a.MedicalRecords)
             .HasForeignKey(x => x.AnimalId)
             .OnDelete(DeleteBehavior.Cascade);

            // Indeksy pod typowe zapytania i projekcje
            builder.HasIndex(x => new { x.AnimalId, x.Type });
            builder.HasIndex(x => x.NextDueDate);
        }
    }
}
