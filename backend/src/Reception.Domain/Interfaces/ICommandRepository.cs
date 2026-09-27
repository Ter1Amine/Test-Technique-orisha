using Reception.Core.Enums;
using Reception.Domain.Models;

namespace Reception.Domain.Interfaces
{
    public interface ICommandRepository
    {
        public Task<IEnumerable<Command>> GetCommandsByStatus(CommandStatus status);
    }
}
