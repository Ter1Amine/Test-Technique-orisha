
using Reception.Core.Dtos;
namespace Reception.Application.Interfaces;

public interface ICommandService
{
    Task<CommandDto> GetCommandById(string commandId);
}
