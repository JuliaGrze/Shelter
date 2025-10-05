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
        public AnimalSortField SortBy { get; set; } = AnimalSortField.createdAt;
        public SortDirection SortDir { get; set; } = SortDirection.desc;

        //pagination
        public int Page { get; set; } = 0;
        public int Size { get; set; } = 20;

    }
}
