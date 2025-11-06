using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos.Adoptions
{
    /// <summary>Odpowiedź po złożeniu wniosku.</summary>
    public class SubmitApplicationResponse
    {
        public int ApplicationId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string StatusCode { get; set; } = default!; // np. "Submitted"
        public string StatusName { get; set; } = default!; // np. "Złożony wniosek"
    }
}
