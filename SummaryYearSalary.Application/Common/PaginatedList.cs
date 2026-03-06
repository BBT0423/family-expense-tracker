using System.Collections.ObjectModel;

namespace SummaryYearSalary.Application.Common;

public class PaginatedList<T>
{
    public ReadOnlyCollection<T> Items { get; }
    public int PageNumber { get; set; }
    public int TotalPages { get; set; }
    public int TotalCount { get; set; }

    public PaginatedList() // Add this parameterless constructor
    {
        Items = new ReadOnlyCollection<T>([]);
    }

    public PaginatedList(ReadOnlyCollection<T> items, int count, int pageNumber, int pageSize)
    {
        PageNumber = pageNumber;
        TotalPages = (int)Math.Ceiling(count / (double)pageSize);
        TotalCount = count;
        Items = items;
    }

    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}