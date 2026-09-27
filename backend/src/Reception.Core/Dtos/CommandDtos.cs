namespace Reception.Core.Dtos;

public record ProductDto(
    string RefId,
    string Name,
    string Color,
    string Size,
    int Quantity,
    bool IsReceived,
    string Satus);

public record CartonDto(
    string CartonId,
    string PaletteId,
    string Status,
    string ReceivedPercent,
    IReadOnlyCollection<ProductDto> Products);

public record PaletteDto(
    string PaletteId,
    string CommandId,
    string Status,
    string ReceivedPercent,
    IReadOnlyCollection<CartonDto> Cartons);

public record CommandDto(
    string CommandId,
    string Status,
    string ReceivedPercent,
    IReadOnlyCollection<PaletteDto> Palettes);
