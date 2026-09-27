using Reception.Application.Interfaces;
using Reception.Core.Dtos;
using Reception.Domain.Interfaces;

namespace Reception.Application.Services;

public class CartonReceiptService : ICartonReceiptService
{
    private readonly ICommandRepository _commandRepository;
    public CartonReceiptService(ICommandRepository commandRepository)
    {
        _commandRepository = commandRepository;
    }

    public async Task<CommandDto?> UpdateReceipt(string commandId, string cartonId, bool isReceived)
    {
        var command = await _commandRepository.GetCommandById(commandId);
        var carton = command?.Palettes
            .SelectMany(palette => palette.Cartons)
            .SingleOrDefault(item => item.CartonId == cartonId);
        if (command is null || carton is null)
            return null;

        foreach (var product in carton.Products)
            product.IsReceived = isReceived;

        await _commandRepository.SaveChangeAsync();
        return CommandDtoMapper.ToDto(command);
    }
}