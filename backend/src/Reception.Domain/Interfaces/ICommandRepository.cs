using Reception.Core.Enums;
using Reception.Domain.Models;

namespace Reception.Domain.Interfaces
{
    public interface ICommandRepository
    {
        public Task<Command> GetCommandById(string commandId);
        public Task<IReadOnlyList<Command>> GetAllCommands();
        public Task<IReadOnlyList<string>> GetExistingIds(
            string commandId,
            IReadOnlyCollection<string> paletteIds,
            IReadOnlyCollection<string> cartonIds,
            IReadOnlyCollection<string> productIds);
        public Task AddCommand(Command command);
        public Task SaveChangeAsync();
    }
}
