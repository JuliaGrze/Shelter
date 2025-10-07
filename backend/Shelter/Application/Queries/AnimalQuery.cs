using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Queries
{
    public class AnimalQuery
    {
        //sorting
        public AnimalSortField SortBy { get; set; } = AnimalSortField.createdAt;
        public SortDirection SortDir { get; set; } = SortDirection.desc;

        //pagination
        public int Page { get; set; } = 0;
        public int Size { get; set; } = 20;

        //filters
        public int? SpeciesId { get; set; }
        public Sex? Sex { get; set; }
        public AnimalStatus? Status { get; set; }

        //age in months
        public int? AgeMinMonths { get; set; }         // np. 6
        public int? AgeMaxMonths { get; set; }         // np. 120

        // zakres createdAt
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }

        // flags
        public bool? Vaccinated { get; set; }
        public bool? Neutered { get; set; }


    }
}
