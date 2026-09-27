using Microsoft.AspNetCore.Mvc;
using Reception.Application.Interfaces;
using Reception.Core.Dtos;

namespace Reception.Api.Controllers;

[ApiController]
[Route("api")]
public class Controller : ControllerBase
{
    private readonly ICommandService _commandService;
    private readonly IPaletteReceiptService _paletteReceiptService;
    private readonly ICartonReceiptService _cartonReceiptService;
    private readonly IProductReceiptService _productReceiptService;

    public Controller(
        ICommandService commandService,
        IPaletteReceiptService paletteReceiptService,
        ICartonReceiptService cartonReceiptService,
        IProductReceiptService productReceiptService)
    {
        _commandService = commandService;
        _paletteReceiptService = paletteReceiptService;
        _cartonReceiptService = cartonReceiptService;
        _productReceiptService = productReceiptService;
    }

    [HttpGet("command/{commandId}")]
    [ProducesResponseType(typeof(CommandDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<CommandDto>> GetCommandById(string commandId)
    {
        var command = await _commandService.GetCommandById(commandId);
        if (command is null)
            return NotFound();

        return Ok(command);
    }

    [HttpPut("{commandId}/palettes/{paletteId}/receipt")]
    [ProducesResponseType(typeof(CommandDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<CommandDto>> UpdatePaletteReceipt(
        string commandId,
        string paletteId,
        bool isReceived)
    {
        var command = await _paletteReceiptService.UpdateReceipt(commandId, paletteId, isReceived);
        return command is null ? NotFound() : Ok(command);
    }

    [HttpPut("{commandId}/cartons/{cartonId}/receipt")]
    [ProducesResponseType(typeof(CommandDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<CommandDto>> UpdateCartonReceipt(
        string commandId,
        string cartonId,
        bool isReceived)
    {
        var command = await _cartonReceiptService.UpdateReceipt(commandId, cartonId, isReceived);
        return command is null ? NotFound() : Ok(command);
    }

    [HttpPut("{commandId}/products/{productId}/receipt")]
    [ProducesResponseType(typeof(CommandDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<CommandDto>> UpdateProductReceipt(
        string commandId,
        string productId,
        bool isReceived)
    {
        var command = await _productReceiptService.UpdateReceipt(commandId, productId, isReceived);
        return command is null ? NotFound() : Ok(command);
    }
}
