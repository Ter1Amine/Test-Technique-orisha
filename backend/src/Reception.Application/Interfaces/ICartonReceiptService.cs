using Reception.Core.Dtos;

namespace Reception.Application.Interfaces;

public interface ICartonReceiptService
{
    Task<CommandDto?> UpdateReceipt(string commandId, string cartonId, bool isReceived);
}