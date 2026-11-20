using Microsoft.AspNetCore.Mvc;
using WebEventCompany.Services;

namespace WebEventCompany.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LuckyDrawController : ControllerBase
{
    private readonly ILuckyDrawUserService _userService;
    private readonly ILogger<LuckyDrawController> _logger;

    public LuckyDrawController(ILuckyDrawUserService userService, ILogger<LuckyDrawController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    [HttpGet("GetParticipants")]
    public async Task<IActionResult> GetParticipants(CancellationToken cancellationToken)
    {
        try
        {
            var participants = await _userService.GetParticipantsAsync(cancellationToken);
            return Ok(participants);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Không tải được danh sách user từ Supabase");
            return StatusCode(500, new { message = ex.Message });
        }
    }

    [HttpGet("GetWinners")]
    public IActionResult GetWinners()
    {
        return Ok(Array.Empty<object>());
    }

    [HttpPost("SaveWinner")]
    public IActionResult SaveWinner()
    {
        return Ok();
    }

    [HttpPost("ResetWinners")]
    public IActionResult ResetWinners()
    {
        return Ok();
    }
}
