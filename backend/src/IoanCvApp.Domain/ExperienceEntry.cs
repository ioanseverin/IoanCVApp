namespace IoanCvApp.Domain;

public class ExperienceEntry
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Company { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public string Description { get; set; } = string.Empty;
    public List<string> TechTags { get; set; } = [];
    public int SortOrder { get; set; }
}
