namespace IoanCvApp.Domain;

/// <summary>Represents the single administrator account used to authenticate content management.</summary>
public class AdminUser
{
    /// <summary>Gets or sets the administrator identifier.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the administrator username.</summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>Gets or sets the administrator password hash.</summary>
    public string PasswordHash { get; set; } = string.Empty;
}
