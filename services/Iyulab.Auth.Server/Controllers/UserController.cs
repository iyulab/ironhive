using Iyulab.Auth.Shared;
using Iyulab.Auth.Server.Services;
using Microsoft.AspNetCore.Mvc;

namespace Iyulab.Auth.Server.Controllers;

[ApiController]
[Route("api/users")]
public class UserController : ControllerBase
{
    private readonly UserService _service;

    public UserController(UserService service)
    {
        _service = service;
    }

    [HttpPut]
    public async Task<IActionResult> UpdateAsync(
        [FromBody] User user)
    {
        await _service.UpdateAsync(user);
        return Ok();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(
        [FromRoute] Guid id)
    {
        await _service.DeleteAsync(id);
        return Ok();
    }
}
