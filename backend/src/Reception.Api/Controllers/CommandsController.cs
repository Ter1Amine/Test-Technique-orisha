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

    [HttpGet("command/{commandId}")]
    [ProducesResponseType(typeof(CommandDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<CommandDto>> GetCommandById(string commandId)
    {
        var command = await _commandService.GetCommandById(commandId);
        return Ok(command);
    }
}
