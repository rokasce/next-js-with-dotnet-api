namespace API.Entities;

public class RefreshToken
{
    public int Id { get; set; }
    public ApiUser? ApiUser { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime Expires { get; set; } = DateTime.UtcNow.AddDays(7);
    public bool IsExpired => DateTime.UtcNow >= Expires;
    public DateTime? Revoked { get; set; }
    public bool IsActive => Revoked == null && !IsExpired;
}