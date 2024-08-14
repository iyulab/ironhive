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
        [FromHeader(Name = Constants.UserHeader)] Guid userId)
    {
        var user = await _repo.GetByIdAsync(userId);
        user = user is null
            ? await _repo.AddAsync(new User { ID = userId })
            : await _repo.UpdateAsync(user);
        
        return Ok(user);
    }
}
