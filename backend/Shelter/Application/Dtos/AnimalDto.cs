using Domain.Entities;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos
{
    public class AnimalDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public int SpeciesId { get; set; } //Foreign Key
        public string SpeciesName { get; set; } = "";


        public DateOnly BirthDate { get; set; } //we calculate age from date of birth
        public Sex Sex { get; set; } 
        public AnimalStatus Status { get; set; } //Available, Reserved, Adopted, NotAvailable

        public string Description { get; set; } = "";
        public bool Vaccinated { get; set; } //zaszczepiony?
        public bool Neutered { get; set; } //kastrowany/sterilizowany?  
        public DateTime CreatedAt { get; set; } 
        public string PhotoUrl { get; set; } = "";
    }
}
