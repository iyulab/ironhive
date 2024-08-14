using Microsoft.AspNetCore.Mvc;
using Raggle.Server.Web.Database;
using Raggle.Server.Web.Models;

namespace Raggle.Server.Web.Controllers;

[Route("/api/assistant")]
[ApiController]
public class AssistantController : ControllerBase
{
    private readonly AppRepository<Assistant> _repo;

    public AssistantController(AppRepository<Assistant> assistantRepository)
    {
        _repo = assistantRepository;
    }

    [HttpGet]
    public async Task<ActionResult<Assistant>> GetAssistantAsync(
        [FromHeader(Name = Constants.UserHeader)] Guid userId)
    {
        var assistant = await _repo.FindFirstAsync(a => a.UserID == userId);
        assistant ??= await _repo.AddAsync(new Assistant { UserID = userId });
        return Ok(assistant);
    }

    [HttpPut]
    public async Task<ActionResult<Assistant>> PutAssistantAsync(
        [FromBody] Assistant assistant)
    {
        assistant = await _repo.UpdateAsync(assistant);
        return Ok(assistant);
    }
}
