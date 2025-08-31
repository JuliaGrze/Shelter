using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Species
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;

        //Relationships one-to-many
        public ICollection<Animal> Animals { get; set; } = new List<Animal>();
    }
}
