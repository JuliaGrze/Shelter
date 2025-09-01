using Application.Dtos;
using Domain.Entities;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Mapping
{
    public static class AnimalMapping
    {
        // Entity -> DTO (READ)
        public static AnimalDto AnimalToDto(Animal animal) => new AnimalDto
        {
            Id = animal.Id,
            Name = animal.Name,
            SpeciesId = animal.SpeciesId,
            SpeciesName = animal.Species.Name,
            BirthDate = animal.BirthDate,
            Sex = Enum.TryParse<Sex>(animal.Sex, true, out var sx) ? sx : Sex.Unknown,
            Status = Enum.TryParse<AnimalStatus>(animal.Status, true, out var st) ? st : AnimalStatus.NotAvailable,
            Description = animal.Description ?? "",
            Vaccinated = animal.Vaccinated,
            Neutered = animal.Neutered,
            CreatedAt = animal.CreatedAt,
            PhotoUrl = animal.PhotoUrl ?? ""
        };

        public static Animal DtoToAnimal(CreateAnimalDto animalDto) => new Animal
        {
            Name = animalDto.Name,
            SpeciesId = animalDto.SpeciesId,
            BirthDate = animalDto.BirthDate,
            Sex = animalDto.Sex.ToString(),
            Status = animalDto.Status.ToString(),
            Description = animalDto.Description ?? "",
            Vaccinated = animalDto.Vaccinated,
            Neutered = animalDto.Neutered,
            PhotoUrl = animalDto.PhotoUrl ?? ""
        };
        
    }
}
