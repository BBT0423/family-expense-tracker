namespace SummaryYearSalary.Api.Configurations;

public class FileStorageOptions
{
    public const string SectionName = "FileStorage";
    public string UploadsPath { get; set; } = "Public/uploads";
    public string BaseUrl { get; set; } = "";
}