namespace SummaryYearSalary.Application.Common;

public class CreatedResult(int id, string? no = null)
{
    public int Id { get; set; } = id;
    public string? No { get; set; } = no;

    public static CreatedResult FromId(int id) => new(id);
    public static CreatedResult FromIdAndNo(int id, string no) => new(id, no);
}