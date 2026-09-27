using Reception.Core.Dtos;

namespace Reception.Application.Interfaces;

public interface IProductReceiptService
{
    Task<CommandDto?> UpdateReceipt(string commandId, string productId, bool isReceived);
}