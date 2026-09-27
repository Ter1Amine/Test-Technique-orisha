using Reception.Application.Interfaces;
using Reception.Core.Dtos;
using Reception.Domain.Interfaces;

namespace Reception.Application.Services;

public class ProductReceiptService : IProductReceiptService
{
    private readonly ICommandRepository _commandRepository;

    public ProductReceiptService(ICommandRepository commandRepository)
    {
        _commandRepository = commandRepository;
    }
    public async Task<CommandDto?> UpdateReceipt(string commandId, string productId, bool isReceived)
    {
        var command = await _commandRepository.GetCommandById(commandId);
        var product = command?.Palettes
            .SelectMany(palette => palette.Cartons)
            .SelectMany(carton => carton.Products)
            .SingleOrDefault(item => item.RefId == productId);
        if (command is null || product is null)
            return null;

        product.IsReceived = isReceived;
        await _commandRepository.SaveChangeAsync();
        return CommandDtoMapper.ToDto(command);
    }
}