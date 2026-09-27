using Reception.Application.Interfaces;
using Reception.Core.Dtos;
using Reception.Domain.Interfaces;

namespace Reception.Application.Services;

public class PaletteReceiptService : IPaletteReceiptService
{
    private readonly ICommandRepository _commandRepository;

    public PaletteReceiptService(ICommandRepository commandRepository)
    {
        _commandRepository = commandRepository;
    }

    public async Task<CommandDto?> UpdateReceipt(string commandId, string paletteId, bool isReceived)
    {
        var command = await _commandRepository.GetCommandById(commandId);
        var palette = command?.Palettes.SingleOrDefault(item => item.PaletteId == paletteId);
        if (command is null || palette is null)
            return null;

        foreach (var product in palette.Cartons.SelectMany(carton => carton.Products))
            product.IsReceived = isReceived;

        await _commandRepository.SaveChangeAsync();
        return CommandDtoMapper.ToDto(command);
    }
}