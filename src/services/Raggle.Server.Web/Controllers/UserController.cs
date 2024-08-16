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
        [FromHeader(Name = Constants.UserHeader)] Guid userId,
        [FromHeader(Name = "User-Agent")] string? deviceInfo,
        [FromHeader(Name = "Accept-Language")] string? locale)
    {
        Console.WriteLine(Request);
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString().Replace("::1", "localhost");
        var user = await _repo.GetByIdAsync(userId);
        user = user is null
            ? await _repo.AddAsync(new User 
            { 
                ID = userId, 
                DeviceInfo = deviceInfo, 
                Locale = locale, 
                IPAddress = ip 
            })
            : await _repo.UpdateAsync(user);
        
        return Ok(user);
    }
}
