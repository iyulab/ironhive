using Microsoft.AspNetCore.Mvc;
using Raggle.Server.API.Models;
using Raggle.Server.API.Repositories;

namespace Raggle.Server.API.Controllers
{
    [ApiController]
    [Route("/user")]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;
        private readonly UserRepository _user;

        public UserController(ILogger<UserController> logger, UserRepository user)
        {
            _logger = logger;
            _user = user;
        }

        [HttpGet("{userId:guid}")]
        public async Task<IActionResult> GetUser(Guid userId)
        {
            var user = await _user.GetUser(userId);
            return Ok(user);
        }

        [HttpPut("{userId:guid}")]
        public async Task<IActionResult> UpdateUser(Guid userId, [FromBody] User user)
        {
            await _user.UpdateUser(user);
            return Ok();
        }
    }
}
