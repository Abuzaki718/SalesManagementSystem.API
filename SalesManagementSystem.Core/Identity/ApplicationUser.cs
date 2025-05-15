using Microsoft.AspNetCore.Identity;

namespace Murur.Core.Domain.Identity;

public class ApplicationUser : IdentityUser
{

    public bool IsEnabled { get; set; }
    public string FullName { get; set; } = null!;



}
