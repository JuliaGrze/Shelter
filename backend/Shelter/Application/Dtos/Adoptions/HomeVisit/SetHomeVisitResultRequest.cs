using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos.Adoptions.HomeVisit
{
    public class SetHomeVisitResultRequest
    {
        [Required, Range(1, int.MaxValue)]
        public int HomeVisitResultId { get; set; }

        [StringLength(1000)] 
        public string? Notes { get; set; }

    }
}
