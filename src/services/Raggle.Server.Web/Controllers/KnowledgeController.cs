using Microsoft.AspNetCore.Mvc;
using Raggle.Server.Web.Database;
using Raggle.Server.Web.Models;

namespace Raggle.Server.Web.Controllers;

[Route("/api/knowledge")]
[ApiController]
public class KnowledgeController : ControllerBase
{
    private readonly AppRepository<Knowledge> _repo;

    public KnowledgeController(AppRepository<Knowledge> knowledgeRepository)
    {
        _repo = knowledgeRepository;
    }

    [HttpGet("/api/knowledges")]
    public async Task<ActionResult<IEnumerable<Knowledge>>> GetKnowledgesAsync(
        [FromHeader(Name = Constants.UserHeader)] Guid userId,
        [FromQuery] int skip = 0,
        [FromQuery] int limit = 10)
    {
        var knowledges = await _repo.FindAsync(
            k => k.UserID == userId,                // 조건: 특정 사용자 ID
            k => k.CreatedAt,                       // 정렬: 생성일자 기준
            descending: true,                       // 내림차순 정렬 (최신 순)
            skip: skip,                             // 건너뛸 항목 수
            limit: limit                            // 반환할 항목 수
        );

        return Ok(knowledges);
    }

    [HttpGet("{knowledgeId:guid}")]
    public async Task<ActionResult<Knowledge>> GetKnowledgeAsync(Guid knowledgeId)
    {
        var knowledge = await _repo.GetByIdAsync(knowledgeId);
        if (knowledge == null)
        {
            return NotFound();
        }
        return Ok(knowledge);
    }

    [HttpPost]
    public async Task<ActionResult<Knowledge>> PostKnowledgeAsync(
        [FromHeader(Name = Constants.UserHeader)] Guid userId,
        [FromBody] Knowledge knowledge)
    {
        knowledge.UserID = userId;
        knowledge = await _repo.AddAsync(knowledge);
        return CreatedAtAction(nameof(GetKnowledgeAsync), new { knowledgeId = knowledge.ID }, knowledge);
    }

    [HttpPut]
    public async Task<ActionResult<Knowledge>> PutKnowledgeAsync([FromBody] Knowledge knowledge)
    {
        knowledge = await _repo.UpdateAsync(knowledge);
        return Ok(knowledge);
    }

    [HttpDelete("{knowledgeId:guid}")]
    public async Task<IActionResult> DeleteKnowledgeAsync(Guid knowledgeId)
    {
        try
        {
            await _repo.DeleteAsync(knowledgeId);
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

