namespace TaskFlow.Domain.Entities;

public class User
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Email { get; private set; } = default!;
    public string DisplayName { get; private set; } = default!;
    public string PasswordHash { get; private set; } = default!;
    public string? RefreshToken { get; private set; }
    public DateTime? RefreshTokenExpiry { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    private User() { }

    public static User Create(string email, string displayName, string passwordHash) =>
        new() { Email = email.ToLower(), DisplayName = displayName, PasswordHash = passwordHash };

    public void SetRefreshToken(string token, DateTime expiry)
    {
        RefreshToken = token;
        RefreshTokenExpiry = expiry;
    }

    public void RevokeRefreshToken()
    {
        RefreshToken = null;
        RefreshTokenExpiry = null;
    }
}