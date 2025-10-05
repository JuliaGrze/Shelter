using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common
{
    /// <summary>
    /// Represents a generic paged result used for returning lists of data
    /// together with pagination metadata (total count, current page, page size).
    /// This allows the frontend to display paginated data
    /// without fetching all records at once.
    /// </summary>
    /// <typeparam name="T">
    /// The type of items contained in the paged result (e.g. AnimalDto).
    /// </typeparam>
    public record PagedResult<T>(IReadOnlyList<T> Items, int Total, int Page, int Size);
}
