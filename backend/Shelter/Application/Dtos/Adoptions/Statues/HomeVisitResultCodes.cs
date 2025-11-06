using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos.Adoptions.Statues
{
    public static class HomeVisitResultCodes
    {
        public const string Pending = "Pending";
        public const string Passed = "Passed";
        public const string Failed = "Failed";
        public const string Cancelled = "Cancelled";
        public const string Rescheduled = "Rescheduled";
    }
}
