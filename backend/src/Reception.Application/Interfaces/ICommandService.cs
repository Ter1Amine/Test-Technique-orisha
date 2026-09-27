
using Reception.Core.Dtos;
namespace Reception.Application.Interfaces;

public interface ICommandService
{
    Task<IEnumerable<CommandDto>> GetInProgressCommandsAsync();
}
