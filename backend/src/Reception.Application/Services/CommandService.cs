using Reception.Application.Interfaces;
using Reception.Core.Dtos;
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

    public async Task<CommandDto?> GetCommandById(string commandId)
    {
        var command = await _commandRepository.GetCommandById(commandId);
        return command is null ? null : CommandDtoMapper.ToDto(command);
    }

    public async Task<IReadOnlyList<CommandDto>> GetAllCommands()
    {
        var commands = await _commandRepository.GetAllCommands();
        return commands.Select(CommandDtoMapper.ToDto).ToList();
    }

    public async Task<CommandDto?> UpdateReceipt(string commandId, bool isReceived)
    {
        var command = await _commandRepository.GetCommandById(commandId);
        if (command is null)
            return null;

        foreach (var product in command.Palettes.SelectMany(p => p.Cartons).SelectMany(c => c.Products))
            product.IsReceived = isReceived;

        await _commandRepository.SaveChangeAsync();
        return CommandDtoMapper.ToDto(command);
    }

    public async Task<CreateCommandResult> CreateCommand(CreateCommandRequest request)
    {
        var errors = Validate(request);
        if (errors.Count > 0)
            return new CreateCommandResult(null, errors);

        var palettes = request.Palettes;
        var cartons = palettes.SelectMany(p => p.Cartons).ToList();
        var products = cartons.SelectMany(c => c.Products).ToList();

        var existingIds = await _commandRepository.GetExistingIds(
            request.CommandId,
            palettes.Select(p => p.PaletteId).ToList(),
            cartons.Select(c => c.CartonId).ToList(),
            products.Select(p => p.RefId).ToList());
        if (existingIds.Count > 0)
            return new CreateCommandResult(null, existingIds.Select(id => $"{id} existe déjà.").ToList());

        var command = new Command
        {
            CommandId = request.CommandId.Trim(),
            Palettes = palettes.Select(p => new Palette
            {
                PaletteId = p.PaletteId.Trim(),
                Cartons = p.Cartons.Select(c => new Carton
                {
                    CartonId = c.CartonId.Trim(),
                    Products = c.Products.Select(pr => new Product
                    {
                        RefId = pr.RefId.Trim(),
                        Name = pr.Name.Trim(),
                        Color = pr.Color.Trim(),
                        Size = pr.Size.Trim(),
                        Quantity = pr.Quantity,
                        IsReceived = false
                    }).ToList()
                }).ToList()
            }).ToList()
        };

        await _commandRepository.AddCommand(command);
        return new CreateCommandResult(CommandDtoMapper.ToDto(command), []);
    }

    private static List<string> Validate(CreateCommandRequest request)
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(request.CommandId))
            errors.Add("L'identifiant de la commande est obligatoire.");

        var palettes = request.Palettes ?? [];
        if (palettes.Count == 0)
            errors.Add("La commande doit contenir au moins une palette.");

        var ids = new HashSet<string>();
        void CheckId(string? id, string label)
        {
            if (string.IsNullOrWhiteSpace(id))
                errors.Add($"L'identifiant {label} est obligatoire.");
            else if (!ids.Add(id.Trim()))
                errors.Add($"L'identifiant {id} est utilisé plusieurs fois.");
        }

        foreach (var palette in palettes)
        {
            CheckId(palette.PaletteId, "de palette");
            var cartons = palette.Cartons ?? [];
            if (cartons.Count == 0)
                errors.Add($"La palette {palette.PaletteId} doit contenir au moins un carton.");

            foreach (var carton in cartons)
            {
                CheckId(carton.CartonId, "de carton");
                var products = carton.Products ?? [];
                if (products.Count == 0)
                    errors.Add($"Le carton {carton.CartonId} doit contenir au moins un produit.");

                foreach (var product in products)
                {
                    CheckId(product.RefId, "de produit");
                    if (string.IsNullOrWhiteSpace(product.Name)
                        || string.IsNullOrWhiteSpace(product.Color)
                        || string.IsNullOrWhiteSpace(product.Size))
                        errors.Add($"Le produit {product.RefId} doit avoir un nom, une couleur et une taille.");
                    if (product.Quantity <= 0)
                        errors.Add($"La quantité du produit {product.RefId} doit être supérieure à 0.");
                }
            }
        }

        return errors;
    }
}
