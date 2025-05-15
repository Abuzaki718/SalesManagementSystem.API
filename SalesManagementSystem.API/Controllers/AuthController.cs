using Microsoft.AspNetCore.Mvc;
using SalesManagementSystem.Core.Interfaces.Services.Auth;
using SalesManagementSystem.Shared.DataTransferObjects.Auth;
using SalesManagementSystem.Shared.ResponseModles;

namespace SalesManagementSystem.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{

    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }


    [HttpPost("login")]

    public async Task<ActionResult<BaseResponse<AuthModel>>> Login([FromBody] LoginModel model)
    {
        var result = await _authService.Login(model);
        if (result.IsAuthenticated)
        {
            return Ok(new BaseResponse<AuthModel>(result, "Login Success!"));
        }
        else
        {
            return BadRequest(new BaseResponse<AuthModel>(result, result.Message ?? "", success: false));
        }
    }

    [HttpPost("register")]

    public async Task<ActionResult<BaseResponse<AuthModel>>> Register([FromBody] RegisterDto Dto)
    {
        var result = await _authService.CreateAccount(Dto);
        if (result.Success)
        {
            return Ok(result);
        }
        else
        {
            return BadRequest(result);
        }
    }

}
