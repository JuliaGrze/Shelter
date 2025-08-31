using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

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
        Console.WriteLine($"Species count: {await db.Species.CountAsync()}");
        Console.WriteLine($"Species file exists: {File.Exists(speciesPath)}");
        Console.WriteLine($"Looking for species file at: {speciesPath}");



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
        Console.WriteLine($"Animals count: {await db.Animals.CountAsync()}");
        Console.WriteLine($"Animals file exists: {File.Exists(animalsPath)}");
    }
}
