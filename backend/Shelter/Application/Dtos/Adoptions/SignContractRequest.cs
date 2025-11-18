using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos.Adoptions
{
    public class SignContractRequest
    {
        public string SignatureBase64 { get; set; } = default!;
    }
}
