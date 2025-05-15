using System.ComponentModel.DataAnnotations;

namespace SalesManagementSystem.Shared.DataTransferObjects.Auth;

public class LoginModel
{
    [Required(ErrorMessage = "يرجى اخال  اسم المستخدم المسجل به مسبقا")]
    public string Username { get; set; } = null!;

    [Required(ErrorMessage = "يرجى اخال كلمه السر")]

    public string Password { get; set; } = null!;

}

public class RegisterDto
{
    [Required(ErrorMessage = "يرجى اخال  اسمك")]
    public string FullName { get; set; } = null!;

    public string Phone { get; set; }

    [Required(ErrorMessage = "يرجى اخال  اسم المستخدم")]
    public string Username { get; set; } = null!;

    [Required(ErrorMessage = "يرجى اخال كلمه السر")]

    public string Password { get; set; } = null!;
}