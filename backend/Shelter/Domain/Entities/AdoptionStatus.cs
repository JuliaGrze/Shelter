using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class AdoptionStatus
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty; // np. "Submitted"
        public string Name { get; set; } = string.Empty; // np. "Zlozony wniosek"
        public string? Description { get; set; }
        public bool IsFinal { get; set; }

    }
}
