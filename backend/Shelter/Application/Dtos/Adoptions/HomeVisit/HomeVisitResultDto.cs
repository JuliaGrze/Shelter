using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos.Adoptions.HomeVisit
{
    public class HomeVisitResultDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = String.Empty;
        public string? Description { get; set; }
        public bool IsFinal { get; set; }
    }
}
