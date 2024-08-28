using Microsoft.AspNetCore.Mvc;
using Raggle.Server.Web;
using Raggle.Server.Web.Database;
using Raggle.Server.Web.Models;

[Route("/api/user")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly AppRepository<User> _repo;

    public UserController(AppRepository<User> userRepository)
    {
        _repo = userRepository;
    }

    [HttpGet]
    public async Task<ActionResult<User>> GetUserAsync(
        [FromHeader(Name = Constants.UserHeader)] Guid? userId,
        [FromHeader(Name = "User-Agent")] string? deviceInfo,
        [FromHeader(Name = "Accept-Language")] string? locale)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString().Replace("::1", "localhost");

        User? user = null;
        if (userId.HasValue && userId.Value != Guid.Empty)
        {
            user = await _repo.GetByIdAsync(userId.Value);
        }

        if (user == null)
        {
            user = await _repo.AddAsync(new User
            {
                DeviceInfo = deviceInfo,
                Locale = locale,
                IPAddress = ip
            });
        }
        else
        {
            user.DeviceInfo = deviceInfo;
            user.Locale = locale;
            user.IPAddress = ip;
            user = await _repo.UpdateAsync(user);
        }

        return Ok(user);
    }
}
