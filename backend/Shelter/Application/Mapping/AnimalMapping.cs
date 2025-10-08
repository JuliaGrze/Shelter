using Application.Dtos.Animal;
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
        public static AnimalDto AnimalToDto(Animal animal)
        {
            var (years, months) = CalculateAge(animal.BirthDate);

            return new AnimalDto
            {
                Id = animal.Id,
                Name = animal.Name,
                SpeciesId = animal.SpeciesId,
                SpeciesName = animal.Species?.Name ?? "",   // bezpiecznie na null
                BirthDate = animal.BirthDate,

                Sex = Enum.TryParse<Sex>(animal.Sex, true, out var sx) ? sx : Sex.Unknown,
                Status = Enum.TryParse<AnimalStatus>(animal.Status, true, out var st) ? st : AnimalStatus.NotAvailable,

                Description = animal.Description ?? "",
                Vaccinated = animal.Vaccinated,
                Neutered = animal.Neutered,
                CreatedAt = animal.CreatedAt,
                PhotoUrl = animal.PhotoUrl ?? "",

                // ⬇️ nowe pola
                AgeYears = years,
                AgeMonths = months,
                AgeLabel = FormatAgeLabel(years, months)
            };
        }

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

        private static (int years, int months) CalculateAge(DateOnly birth)
        {
            // zabezpieczenia
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            if (birth > today)
                return (0, 0);

            int years = today.Year - birth.Year;
            int months = today.Month - birth.Month;

            // jeśli bieżący dzień miesiąca < dnia urodzin -> miesiąc jeszcze nie „zaliczony”
            if (today.Day < birth.Day)
                months--;

            if (months < 0)
            {
                years--;
                months += 12;
            }

            if (years < 0) { years = 0; months = 0; }

            return (years, months);
        }

        private static string FormatAgeLabel(int years, int months)
        {
            if (years == 0)
                return $"{months} mies.";

            string yearWord = GetYearWord(years);
            string monthPart = months > 0 ? $", {months} mies." : "";

            return $"{years} {yearWord}{monthPart}";
        }

        private static string GetYearWord(int years)
        {
            // zasada: 1 -> "rok", 2-4 (z wyjątkiem 12-14) -> "lata", reszta -> "lat"
            int lastDigit = years % 10;
            int lastTwoDigits = years % 100;

            if (lastDigit == 1 && lastTwoDigits != 11)
                return "rok";
            else if (lastDigit >= 2 && lastDigit <= 4 && (lastTwoDigits < 12 || lastTwoDigits > 14))
                return "lata";
            else
                return "lat";
        }

    }
}

