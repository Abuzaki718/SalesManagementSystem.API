using System.Text.Json.Serialization;

namespace SalesManagementSystem.Shared.DataTransferObjects.Auth;

public class AuthModel
{
    public string? Message { get; set; }
    public bool IsAuthenticated { get; set; }
    public List<string>? UserRoles { get; set; }
    public string? Token { get; set; }

    //public DateTime ExpiresOn { get; set; }

    //[JsonIgnore]
    //public string? RefreshToken { get; set; }

    //public DateTime RefreshTokenExpiration { get; set; }

    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? Email { get; set; } = string.Empty;

    //public List<string>? Permissions { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? Errors { get; set; }
}
