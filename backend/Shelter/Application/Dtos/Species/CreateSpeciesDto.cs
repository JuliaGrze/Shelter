using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos.Species
{
    public class CreateSpeciesDto
    {
        public string Name { get; set; } = "";

        public bool RequiresPermit { get; set; }
        public string? PermitName { get; set; }
        public string? PermitAuthority { get; set; }
        public string? PermitNotes { get; set; }
    }
}
