using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos.Adoptions.Statues
{
    public static class AdoptionStatusCodes
    {
        public const string Submitted = "Submitted";
        public const string InReview = "InReview";
        public const string HomeVisitScheduled = "HomeVisitScheduled";
        public const string HomeVisitCompleted = "HomeVisitCompleted";
        public const string Approved = "Approved";
        public const string Rejected = "Rejected";
        public const string Withdrawn = "Withdrawn";
        public const string ContractGenerated = "ContractGenerated";
        public const string ContractSigned = "ContractSigned";
    }
}
