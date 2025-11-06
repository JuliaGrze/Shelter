using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos.Adoptions.HomeVisit
{
    public class ScheduleHomeVisitRequest
    {
        [Required] 
        public DateTime Date { get; set; }

        [StringLength(1000)] 
        public string? Notes { get; set; }
    }
}
