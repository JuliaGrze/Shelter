using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos.Adoptions
{
    public class UpdateAdoptionStatusRequest
    {
        [Required, StringLength(1000)]
        public string Notes { get; set; } = default!;
    }
}
