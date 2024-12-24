using Iyulab.Auth.Shared;
using Microsoft.AspNetCore.Mvc;

namespace Iyulab.Auth.Server.Controllers;

[ApiController]
[Route("/api/auth")]
public class AuthController : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> SignInAsync(SignIn request)
    {
        try
        {
            throw new NotImplementedException();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost]
    public async Task<IActionResult> SignUpAsync(SignUp request)
    {
        try
        {
            throw new NotImplementedException();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost]
    public async Task<IActionResult> SignOutAsync()
    {
        try
        {
            throw new NotImplementedException();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
