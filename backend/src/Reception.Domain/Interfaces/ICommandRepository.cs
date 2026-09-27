using Reception.Core.Enums;
using Reception.Domain.Models;

namespace Reception.Domain.Interfaces
{
    public interface ICommandRepository
    {
        public Task<Command> GetCommandById(string commandId);
    }
}
