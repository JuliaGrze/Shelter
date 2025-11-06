using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos.Adoptions
{
    /// <summary>Żądanie złożenia wniosku adopcyjnego.</summary>
    public class SubmitApplicationRequest
    {
        public int AnimalId { get; set; }

        [StringLength(2000)]
        public string? Notes { get; set; }
    }
}
