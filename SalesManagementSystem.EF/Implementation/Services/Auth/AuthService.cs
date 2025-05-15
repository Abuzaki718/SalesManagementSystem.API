using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Murur.Core.Domain.Identity;
using SalesManagementSystem.Core.Interfaces.Services.Auth;
using SalesManagementSystem.Core.Settings;
using SalesManagementSystem.EF.DataContext;
using SalesManagementSystem.Shared.Constants;
using SalesManagementSystem.Shared.DataTransferObjects.Auth;
using SalesManagementSystem.Shared.ResponseModles;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SalesManagementSystem.EF.Implementation.Services.Auth;

public sealed class AuthService(AppDbContext _context, UserManager<ApplicationUser> _userManager, RoleManager<IdentityRole> _roleManager, JWT_Options _jWT_Options) : IAuthService
{

    public async Task<AuthModel> Login(LoginModel model)
    {

        var userInDB = await _context.Users
                .SingleOrDefaultAsync(x => x.UserName == model.Username);

        // check first if user is in db or if password is not correct

        if (userInDB is null || !await _userManager.CheckPasswordAsync(userInDB, model.Password))
        {
            return new AuthModel { Message = "كلمه المرور او اسم المستخدم خطاء", IsAuthenticated = false, Token = null };
        }
        if (!userInDB.IsEnabled)
        {
            return new AuthModel { Message = "الحساب موقوف", IsAuthenticated = false, Token = null };
        }
        // generate Token
        var jwtSecurityToken = await CreateJwtToken(userInDB);
        IList<string> roleList = await _userManager.GetRolesAsync(userInDB);

        // Map values into AuthModel
        var authModel = new AuthModel
        {
            Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken),
            IsAuthenticated = true,
            FullName = userInDB.FullName,
            Email = userInDB.Email,
            PhoneNumber = userInDB.PhoneNumber,
            UserRoles = (List<string>)roleList,

        };


        return authModel;
    }
    public async Task<BaseResponse<AuthModel>> CreateAccount(RegisterDto Dto)
    {
        AuthModel authModel = new AuthModel();
        var userInDB = await _context.Users
               .SingleOrDefaultAsync(x => x.UserName == Dto.Username);

        if (userInDB != null)
        {
            authModel.Message = "User already exists";
            return new BaseResponse<AuthModel>(authModel, authModel.Message, success: false);
        }


        ApplicationUser user = new ApplicationUser
        {
            UserName = Dto.Username,
            PhoneNumber = Dto.Phone,
            FullName = Dto.FullName,
            IsEnabled = true,
        };

        return await CreateUserAsync(user, Dto.Password, RolesNames.User);

    }
    private async Task<BaseResponse<AuthModel>> CreateUserAsync(ApplicationUser userToDB, string password, string roleName)
    {

        // transaction
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var createUserResult = await _userManager.CreateAsync(userToDB, password);
            if (!createUserResult.Succeeded)
            {

                return new BaseResponse<AuthModel>(new AuthModel { Message = "Failed to create user.", IsAuthenticated = false, Token = null, Errors = createUserResult.Errors.Select(e => e.Description).ToList() }, "Failed to create user.", success: false);
            }

            // Step 5: Assign roles and permissions
            var roleResult = await _userManager.AddToRoleAsync(userToDB, roleName);
            if (!roleResult.Succeeded)
            {

                return new BaseResponse<AuthModel>(new AuthModel { Message = "Failed to assign role to user.", IsAuthenticated = false, Token = null }, "Failed to assign role to user.", success: false);
            }




            // Step 6: Generate JWT token
            var jwtSecurityToken = await CreateJwtToken(userToDB);

            await transaction.CommitAsync();


            var authmodel = new AuthModel
            {
                Message = "User created successfully.",
                FullName = userToDB.FullName,
                Email = userToDB.Email,
                IsAuthenticated = true,
                UserRoles = new List<string> { roleName },
                PhoneNumber = userToDB.PhoneNumber,
                Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken),

            };
            return new BaseResponse<AuthModel>(authmodel, authmodel.Message, success: true);

        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new BaseResponse<AuthModel>(new AuthModel { Message = "Failed to create user.", IsAuthenticated = false, Token = null, Errors = [ex.Message + "-" + ex.InnerException] }, "Failed to create user.", success: false);
        }


    }
    private async Task<JwtSecurityToken> CreateJwtToken(ApplicationUser user)
    {

        var roles = await _userManager.GetRolesAsync(user);
        var roleClaims = new List<Claim>();

        foreach (var role in roles)
            roleClaims.Add(new Claim("roles", role));

        var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
                new Claim("IsEnabled", user.IsEnabled.ToString()),
                new Claim("PhoneNumber", user.PhoneNumber ?? ""),
                new Claim("Id", user.Id),
                //new Claim($"{nameof(ApplicationUser.EntityType)}", user.EntityType.ToString()),
            };

        claims.AddRange(roleClaims);


        var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jWT_Options.SecretKey));
        var signinCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

        var jwtSecurityToken = new JwtSecurityToken(
            issuer: _jWT_Options.Issuer,
            audience: _jWT_Options.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddDays(_jWT_Options.Lifetime),
            signingCredentials: signinCredentials);

        return jwtSecurityToken;
    }

}
