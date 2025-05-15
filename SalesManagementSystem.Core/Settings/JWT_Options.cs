namespace SalesManagementSystem.Core.Settings;

public class JWT_Options
{
    public string Issuer { get; set; } = null!;
    public string Audience { get; set; } = null!;
    public int Lifetime { get; set; }
    public string SecretKey { get; set; } = null!;
}
