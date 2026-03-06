using System.ComponentModel.DataAnnotations;

using MediatR;

namespace SummaryYearSalary.Application.Common;

public abstract record BaseFilterQuery<TResponse> : IRequest<TResponse>
{
    [Range(1, int.MaxValue, ErrorMessage = "Page number must be greater than 0")]
    public int PageNumber { get; init; } = 1;

    [Range(1, 100, ErrorMessage = "Page size must be between 1 and 100")]
    public int PageSize { get; init; } = 10;

    public string? SortField { get; init; }
    public string? SortDirection { get; init; } = ApplicationConstants.SortDirectionDesc;
    public string? SearchTerm { get; init; }
}

public abstract record BaseDateRangeFilterQuery<TResponse> : BaseFilterQuery<TResponse>
{
    public DateTime? DateFrom { get; init; }
    public DateTime? DateTo { get; init; }
}

public abstract class BaseEmailQuery
{
    public string? Template { get; set; }
    public string Origin { get; init; } = null!;
}

public class Actor
{
    public int Id { get; set; } = default!;
    public string? Code { get; set; }
    public string? Name { get; set; }
    public DateTime? ActionDate { get; set; }
    public string? Remarks { get; set; }
    public object? Metadata { get; set; }

    public Actor() { }

    public Actor(int id, string? code, string? name = null, DateTime? actionDate = null, string? remarks = null, object? metadata = null)
    {
        Id = id;
        Code = code;
        Name = name;
        ActionDate = actionDate;
        Remarks = remarks;
        Metadata = metadata;
    }

    public static Actor Create(int id, string? code, string? name = null, DateTime? actionDate = null, string? remarks = null, object? metadata = null)
    {
        return new Actor(id, code, name, actionDate, remarks, metadata);
    }
}

public class StatusObject
{
    public int Id { get; set; }
    public string? Code { get; set; }
    public string? Name { get; set; }
    public string? BgColor { get; set; }
    public string? BorderColor { get; set; }
    public string? FontColor { get; set; }
    public int? FontSize { get; set; }
    private StatusObject() { }

    public static StatusObject Create(
        int id,
        string? code,
        string? name,
        string? bgColor = null,
        string? borderColor = null,
        string? fontColor = null,
        int? fontSize = null
    )
    {
        return new StatusObject
        {
            Id = id,
            Code = code,
            Name = name,
            BgColor = bgColor,
            BorderColor = borderColor,
            FontColor = fontColor,
            FontSize = fontSize
        };
    }
}
