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

    public async Task<IEnumerable<CommandDto>> GetInProgressCommandsAsync()
    {
        var commands = await _commandRepository.GetCommandsByStatus(CommandStatus.InProgress);
        return commands.Select(ToDto).ToList();
    }

    private static CommandDto ToDto(Command command) =>
        new(command.CommandId,
            ((CommandStatus)command.Status).ToString(),
            command.Palettes.Select(ToDto).ToList());

    private static PaletteDto ToDto(Palette palette) =>
        new(palette.PaletteId,
            palette.CommandId,
            palette.Cartons.Select(ToDto).ToList());

    private static CartonDto ToDto(Carton carton) =>
        new(carton.CartonId,
            carton.PaletteId,
            carton.Products.Select(ToDto).ToList());

    private static ProductDto ToDto(Product product) =>
        new(product.RefId,
            product.Name,
            product.Color,
            product.Size,
            product.Quantity);
}
