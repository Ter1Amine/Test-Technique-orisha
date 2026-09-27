using Microsoft.EntityFrameworkCore;
using Reception.Core.Enums;
using Reception.Domain.Interfaces;
using Reception.Domain.Models;
using Reception.Infrastructure.Persistence;

namespace Reception.Infrastructure.Repositories
{
    public class CommandRepository : ICommandRepository
    {
        private readonly ReceptionDbContext _dbContext;

        public CommandRepository(ReceptionDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Command>GetCommandById(string commandId)
        {
            return await _dbContext.Commands
                .Include(c => c.Palettes)
                .ThenInclude(p => p.Cartons)
                .ThenInclude(c => c.Products)
                .Where(c => c.CommandId == commandId)
                .SingleOrDefaultAsync();
        }
    }
}
