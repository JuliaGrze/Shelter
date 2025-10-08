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
                Name = species.Name
            };

        //DTO -> Entity (Create/Update)
        public static Species DtoToSpecies(CreateSpeciesDto speciesDto)
            => new Species
            {
                Name = speciesDto.Name.Trim() ?? string.Empty
            };
    }
}
