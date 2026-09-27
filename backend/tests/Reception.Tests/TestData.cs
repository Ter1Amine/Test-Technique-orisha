using Reception.Domain.Models;

namespace Reception.Tests;

public static class TestData
{
    public static Command BuildCommand(string commandId = "CMD-1", params bool[] receivedStates)
    {
        bool Received(int index) => index < receivedStates.Length && receivedStates[index];

        return new Command
        {
            CommandId = commandId,
            Palettes =
            [
                new Palette
                {
                    PaletteId = "PAL-1",
                    CommandId = commandId,
                    Cartons =
                    [
                        new Carton
                        {
                            CartonId = "CAR-1",
                            PaletteId = "PAL-1",
                            Products =
                            [
                                Product("PRD-1", "CAR-1", Received(0)),
                                Product("PRD-2", "CAR-1", Received(1))
                            ]
                        }
                    ]
                },
                new Palette
                {
                    PaletteId = "PAL-2",
                    CommandId = commandId,
                    Cartons =
                    [
                        new Carton
                        {
                            CartonId = "CAR-2",
                            PaletteId = "PAL-2",
                            Products = [Product("PRD-3", "CAR-2", Received(2))]
                        }
                    ]
                }
            ]
        };
    }

    public static IEnumerable<Product> AllProducts(Command command) =>
        command.Palettes.SelectMany(p => p.Cartons).SelectMany(c => c.Products);

    private static Product Product(string refId, string cartonId, bool isReceived) => new()
    {
        RefId = refId,
        Name = $"Name {refId}",
        Color = "Blue",
        Size = "M",
        Quantity = 1,
        CartonId = cartonId,
        IsReceived = isReceived
    };
}
