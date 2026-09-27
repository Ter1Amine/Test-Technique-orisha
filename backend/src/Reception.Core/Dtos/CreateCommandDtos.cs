namespace Reception.Core.Dtos;

public record CreateProductRequest(
    string RefId,
    string Name,
    string Color,
    string Size,
    int Quantity);

public record CreateCartonRequest(
    string CartonId,
    IReadOnlyList<CreateProductRequest> Products);

public record CreatePaletteRequest(
    string PaletteId,
    IReadOnlyList<CreateCartonRequest> Cartons);

public record CreateCommandRequest(
    string CommandId,
    IReadOnlyList<CreatePaletteRequest> Palettes);

public record CreateCommandResult(CommandDto? Command, IReadOnlyList<string> Errors)
{
    public bool IsSuccess => Errors.Count == 0;
}
