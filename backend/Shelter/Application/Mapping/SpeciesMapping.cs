using Application.Dtos.Species;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Mapping
{
    public static class SpeciesMapping
    {
        // Entity -> DTO (READ)
        public static SpeciesDto SpeciestoDto(Species species)
            => new SpeciesDto
            {
                Id = species.Id,
                Name = species.Name,
                RequiresPermit = species.RequiresPermit,
                PermitName = species.PermitName,
                PermitAuthority = species.PermitAuthority,
                PermitNotes = species.PermitNotes
            };

        //DTO -> Entity (Create/Update)
        public static Species DtoToSpecies(CreateSpeciesDto speciesDto)
            => new Species
            {
                Name = speciesDto.Name.Trim() ?? string.Empty,
                RequiresPermit = speciesDto.RequiresPermit,
                PermitName = string.IsNullOrWhiteSpace(speciesDto.PermitName) ? null : speciesDto.PermitName.Trim(),
                PermitAuthority = string.IsNullOrWhiteSpace(speciesDto.PermitAuthority) ? null : speciesDto.PermitAuthority.Trim(),
                PermitNotes = string.IsNullOrWhiteSpace(speciesDto.PermitNotes) ? null : speciesDto.PermitNotes.Trim(),
            };
    }
}
