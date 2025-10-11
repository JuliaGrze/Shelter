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
    public class SpeciesConfiguration : IEntityTypeConfiguration<Species>
    {
        public void Configure(EntityTypeBuilder<Species> e)
        {
            e.Property(x => x.Name)
               .HasMaxLength(50)
               .IsRequired();

            // Unique name – np. "Dog", "Cat"
            e.HasIndex(x => x.Name).IsUnique();

            //  pozwolenia
            e.Property(x => x.RequiresPermit)
             .HasDefaultValue(false);

            e.Property(x => x.PermitName)
             .HasMaxLength(200);

            e.Property(x => x.PermitAuthority)
             .HasMaxLength(200);

            e.Property(x => x.PermitNotes)
             .HasMaxLength(1000);
        }
    }
}
