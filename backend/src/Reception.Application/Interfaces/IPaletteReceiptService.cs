using Reception.Core.Dtos;

namespace Reception.Application.Interfaces;

public interface IPaletteReceiptService
{
    Task<CommandDto?> UpdateReceipt(string commandId, string paletteId, bool isReceived);
}