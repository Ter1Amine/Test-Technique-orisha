using Microsoft.AspNetCore.Mvc;
using Reception.Application.Interfaces;
using Reception.Core.Dtos;

namespace Reception.Api.Controllers;

[ApiController]
[Route("api/commands")]
public class CommandsController : ControllerBase
{
    private readonly ICommandService _commandService;

    public CommandsController(ICommandService commandService)
    {
        _commandService = commandService;
    }

    [HttpGet("in-progress")]
    [ProducesResponseType(typeof(IEnumerable<CommandDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CommandDto>>> GetInProgressCommands()
    {
        var commands = await _commandService.GetInProgressCommandsAsync();
        return Ok(commands);
    }
}
