
using Reception.Core.Dtos;
namespace Reception.Application.Interfaces;

public interface ICommandService
{
    Task<CommandDto?> GetCommandById(string commandId);
    Task<IReadOnlyList<CommandDto>> GetAllCommands();
    Task<CreateCommandResult> CreateCommand(CreateCommandRequest request);
    Task<CommandDto?> UpdateReceipt(string commandId, bool isReceived);
}
