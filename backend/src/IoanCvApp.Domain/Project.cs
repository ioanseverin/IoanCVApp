namespace IoanCvApp.Domain;

public class Project
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> TechStack { get; set; } = [];
    public string? RepositoryUrl { get; set; }
    public string? LiveUrl { get; set; }
    public string? ImageUrl { get; set; }
    public int SortOrder { get; set; }
}
