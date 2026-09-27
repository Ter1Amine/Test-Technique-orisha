namespace Reception.Core.Dtos;

public record ProductDto(
    string RefId,
    string Name,
    string Color,
    string Size,
    int Quantity);

public record CartonDto(
    string CartonId,
    string PaletteId,
    IReadOnlyCollection<ProductDto> Products);

public record PaletteDto(
    string PaletteId,
    string CommandId,
    IReadOnlyCollection<CartonDto> Cartons);

public record CommandDto(
    string CommandId,
    string Status,
    IReadOnlyCollection<PaletteDto> Palettes);
