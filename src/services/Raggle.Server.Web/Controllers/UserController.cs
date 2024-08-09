using Microsoft.AspNetCore.Mvc;
using Raggle.Server.API.Models;
using Raggle.Server.API.Repositories;
using System.Text.Json;

namespace Raggle.Server.API.Controllers;

[ApiController]
[Route("/user")]
public class UserController : ControllerBase
{
    private readonly UserRepository _user;

    public UserController(UserRepository user)
    {
        _user = user;
    }

    [HttpGet("{userId:guid}")]
    public async Task<IActionResult> GetUserAsync(Guid userId)
    {
        var user = await _user.GetAsync(userId);
        if (user == null) 
        { 
            return NotFound();
        }
        else
        {
            await _user.UpdateAsync(userId, JsonSerializer.SerializeToElement(new
            {
                lastAccessAt = DateTime.UtcNow.ToString("o")
            }));
            return Ok(user);
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateUserAsync([FromBody] User user)
    {
        await _user.InsertAsync(user);
        return Ok();
    }

    [HttpPatch("{userId:guid}")]
    public async Task<IActionResult> UpdateUserAsync(Guid userId, [FromBody] JsonElement updates)
    {
        await _user.UpdateAsync(userId, updates);
        return Ok();
    }
}
