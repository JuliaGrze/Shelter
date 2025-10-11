using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.Json.Serialization;

public static class Seeder
{
    public static async Task SeedAsync(AppDbContext db, string seedDir)
    {
        await db.Database.MigrateAsync();

        var speciesPath = Path.Combine(seedDir, "seed_species.json");
        if (await db.Species.CountAsync() == 0 && File.Exists(speciesPath))
        {
            var json = await File.ReadAllTextAsync(speciesPath);
            var species = JsonSerializer.Deserialize<List<Species>>(json) ?? new();
            foreach (var s in species)
                if (!string.IsNullOrWhiteSpace(s.Name) &&
                    !await db.Species.AnyAsync(x => x.Name == s.Name))
                    db.Species.Add(s);

            await db.SaveChangesAsync();
        }


        var animalsPath = Path.Combine(seedDir, "seed_animals.json");
        if (await db.Animals.CountAsync() == 0 && File.Exists(animalsPath))
        {
            var json = await File.ReadAllTextAsync(animalsPath);
            var animals = JsonSerializer.Deserialize<List<Animal>>(json) ?? new();

            foreach (var a in animals)
            {
                var spExists = await db.Species.AnyAsync(s => s.Id == a.SpeciesId);
                if (!spExists) continue;

                if (!await db.Animals.AnyAsync(x => x.Name == a.Name && x.SpeciesId == a.SpeciesId))
                    db.Animals.Add(a);
            }

            await db.SaveChangesAsync();
        }

        // === NEW: seed medical records ===
        var medPath = Path.Combine(seedDir, "seed_medicalRecords.json");
        if (await db.MedicalRecords.CountAsync() == 0 && File.Exists(medPath))
        {
            // Potrzebny konwerter enumów w JSON (np. "Vaccination" -> MedicalRecordType.Vaccination)
            var opts = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            opts.Converters.Add(new JsonStringEnumConverter());

            var json = await File.ReadAllTextAsync(medPath);
            var records = JsonSerializer.Deserialize<List<MedicalRecord>>(json, opts) ?? new();

            // Bezpieczeństwo: dodaj tylko rekordy, które wskazują na istniejące zwierzę
            // i nie duplikują (AnimalId+Type+Date)
            foreach (var r in records)
            {
                var animalExists = await db.Animals.AnyAsync(a => a.Id == r.AnimalId);
                if (!animalExists) continue;

                var duplicate = await db.MedicalRecords.AnyAsync(m =>
                    m.AnimalId == r.AnimalId &&
                    m.Type == r.Type &&
                    m.Date == r.Date);

                if (!duplicate)
                    db.MedicalRecords.Add(r);
            }

            await db.SaveChangesAsync();
        }
    }
}
