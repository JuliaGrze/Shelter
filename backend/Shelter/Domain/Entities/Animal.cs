using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! W DTOO Sex i Status jako enum, sa juz goitwoe!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    public class Animal
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public int SpeciesId { get; set; } //Foreign Key
        public Species Species { get; set; } = default!; //Navigation Property


        public DateOnly BirthDate { get; set; } //we calculate age from date of birth
        public string Sex { get; set; } = "Unknow";  // "M" / "F" / "Unknown" 
        public string Status { get; set; } = "Available"; //Available, Reserved, Adopted, NotAvailable

        public string? Description { get; set; }
        public bool Vaccinated { get; set; } //zaszczepiony?
        public bool Neutered { get; set; } //kastrowany/sterilizowany?  
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? PhotoUrl { get; set; }
    }
}
