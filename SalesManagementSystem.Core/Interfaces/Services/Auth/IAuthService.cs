using SalesManagementSystem.Core.Settings;
using SalesManagementSystem.Shared.DataTransferObjects.Auth;
using SalesManagementSystem.Shared.ResponseModles;

namespace SalesManagementSystem.Core.Interfaces.Services.Auth;

public interface IAuthService : IScopedInterface
{
    Task<AuthModel> Login(LoginModel model);

    Task<BaseResponse<AuthModel>> CreateAccount(RegisterDto Dto);
}

