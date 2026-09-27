using Reception.Application.Interfaces;
using Reception.Core.Dtos;
using Reception.Core.Enums;
using Reception.Domain.Interfaces;
using Reception.Domain.Models;

namespace Reception.Application.Services;

public class CommandService : ICommandService
{
    private readonly ICommandRepository _commandRepository;

    public CommandService(ICommandRepository commandRepository)
    {
        _commandRepository = commandRepository;
    }

    public async Task<CommandDto> GetCommandById(string commandId)
    {
        var command = await _commandRepository.GetCommandById(commandId);
        return ToDto(command);
    }

    private static CommandDto ToDto(Command command) =>
        new(command.CommandId,
            ((CommandStatus)command.Status).ToString(),
            command.ReceivedPercent,
            command.Palettes.Select(ToDto).ToList());

    private static PaletteDto ToDto(Palette palette) =>
        new(palette.PaletteId,
            palette.CommandId,
            palette.Status.ToString(),
            palette.ReceivedPercent,
            palette.Cartons.Select(ToDto).ToList());

    private static CartonDto ToDto(Carton carton) =>
        new(carton.CartonId,
            carton.PaletteId,
            carton.Status.ToString(),
            carton.ReceivedPercent,
            carton.Products.Select(ToDto).ToList());

    private static ProductDto ToDto(Product product) =>
        new(product.RefId,
            product.Name,
            product.Color,
            product.Size,
            product.Quantity,
            product.IsReceived,
            product.Status.ToString());
}
