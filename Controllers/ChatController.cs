// File: Controllers/ChatController.cs
using Microsoft.AspNetCore.Mvc;
using BrettGravesPortfolio.Services;

namespace BrettGravesPortfolio.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly IResumeRagService _rag;

    public ChatController(IResumeRagService rag)   // <-- inject the interface
    {
        _rag = rag;
    }

    public record ChatRequest(string? question);

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] ChatRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req?.question))
            return BadRequest(new { error = "Question is required." });

        try
        {
            var answer = await _rag.AskAsync(req.question, ct);
            return Ok(new { answer });
        }
        catch (Exception ex)
        {
            // log and return JSON error so the client can show it nicely
            Console.WriteLine(ex);
            return StatusCode(500, new { error = "Chat failed", detail = ex.Message });
        }
    }
}