using Microsoft.AspNetCore.Mvc;
using Raggle.Server.Web.Database;
using Raggle.Server.Web.Models;

namespace Raggle.Server.Web.Controllers;

[Route("/api/connection")]
[ApiController]
public class ConnectionController : ControllerBase
{
    private readonly AppRepository<Connection> _repo;

    public ConnectionController(AppRepository<Connection> connectionRepository)
    {
        _repo = connectionRepository;
    }

    [HttpGet("/api/connections")]
    public async Task<ActionResult<IEnumerable<Connection>>> GetConnectionsAsync(
        [FromHeader(Name = Constants.UserHeader)] Guid userId,
        [FromQuery] int skip = 0,
        [FromQuery] int limit = 10)
    {
        var connections = await _repo.FindAsync(
            c => c.UserID == userId,
            c => c.CreatedAt,
            descending: true,
            skip: skip,
            limit: limit);

        return Ok(connections);
    }

    [HttpGet("{connectionId:guid}")]
    public async Task<ActionResult<Connection>> GetConnectionAsync(Guid connectionId)
    {
        var connection = await _repo.GetByIdAsync(connectionId);
        return Ok(connection);
    }

    [HttpPost]
    public async Task<ActionResult<Connection>> PostConnectionAsync(
        [FromHeader(Name = Constants.UserHeader)] Guid userId,
        [FromBody] Connection connection)
    {
        connection.UserID = userId;
        connection = await _repo.AddAsync(connection);
        return CreatedAtAction(nameof(GetConnectionAsync), new { connectionId = connection.ID }, connection);
    }

    [HttpPut]
    public async Task<ActionResult<Connection>> PutConnectionAsync([FromBody] Connection connection)
    {
        connection = await _repo.UpdateAsync(connection);
        return Ok(connection);
    }

    [HttpDelete("{connectionId:guid}")]
    public async Task<IActionResult> DeleteConnectionAsync(Guid connectionId)
    {
        try
        {
            await _repo.DeleteAsync(connectionId);
            return Ok();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
}
