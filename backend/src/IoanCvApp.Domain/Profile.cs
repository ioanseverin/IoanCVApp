namespace IoanCvApp.Domain;

public class Profile
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Headline { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public List<string> Skills { get; set; } = [];
    public string? LinkedInUrl { get; set; }
    public string? GitHubUrl { get; set; }
    public string? Email { get; set; }
    public string? ResumeUrl { get; set; }
}
