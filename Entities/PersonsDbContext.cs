using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Entities
{
    public class PersonsDbContext : DbContext
    {
        public DbSet<Country> Countries { get; set; }
        public DbSet<Person> Persons { get; set; }

        public PersonsDbContext(DbContextOptions options) : base(options) { }   

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Country>().ToTable("Countries");
            modelBuilder.Entity<Person>().ToTable("Persons");

            // Seed to Countries

            string countriesJson = File.ReadAllText("countries.json");

            List<Country>? countries = JsonSerializer.Deserialize<List<Country>>(countriesJson);

            foreach (Country country in countries!)
                modelBuilder.Entity<Country>().HasData(country);

            // Seed to Persons
            string personsJson = File.ReadAllText("persons.json");

            List<Person>? persons = JsonSerializer.Deserialize<List<Person>>(personsJson);

            foreach (Person person in persons!)
                modelBuilder.Entity<Person>().HasData(person);

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }
    }
}
