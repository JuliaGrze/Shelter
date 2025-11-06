using Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Species> Species { get; set; }
        public DbSet<Animal> Animals { get; set; }
        public DbSet<MedicalRecord> MedicalRecords { get; set; }
        public DbSet<Donation> Donations { get; set; }

        //Adopcja
        public DbSet<AdoptionStatus> AdoptionStatus { get; set; }
        public DbSet<HomeVisitResult> HomeVisitResult { get; set; }
        public DbSet<HomeVisit> HomeVisit { get; set; }
        public DbSet<AdoptionContract> AdoptionContract { get; set; }
        public DbSet<AdoptionApplication> AdoptionApplication { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //wywyoluje metody z klasy bazowej IdentityDbContext, utworza sie tabelki do Logowania itp
            base.OnModelCreating(modelBuilder);

            //EF Core przeskanuje cały assembly (Infrastructure)
            //i znajdzie wszystkie klasy, które implementują IEntityTypeConfiguration<T>
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        }
    }
}
