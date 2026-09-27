using Microsoft.EntityFrameworkCore;
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

        public async Task<Command> GetCommandById(string commandId)
        {
            return await _dbContext.Commands
                .Include(c => c.Palettes)
                .ThenInclude(p => p.Cartons)
                .ThenInclude(c => c.Products)
                .Where(c => c.CommandId == commandId)
                .SingleOrDefaultAsync();
        }

        public async Task<IReadOnlyList<Command>> GetAllCommands()
        {
            return await _dbContext.Commands
                .AsNoTracking()
                .Include(c => c.Palettes)
                .ThenInclude(p => p.Cartons)
                .ThenInclude(c => c.Products)
                .OrderBy(c => c.CommandId)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<string>> GetExistingIds(
            string commandId,
            IReadOnlyCollection<string> paletteIds,
            IReadOnlyCollection<string> cartonIds,
            IReadOnlyCollection<string> productIds)
        {
            var existing = new List<string>();
            if (await _dbContext.Commands.AnyAsync(c => c.CommandId == commandId))
                existing.Add($"Commande {commandId}");

            existing.AddRange((await _dbContext.Palettes
                .Where(p => paletteIds.Contains(p.PaletteId))
                .Select(p => p.PaletteId)
                .ToListAsync()).Select(id => $"Palette {id}"));

            existing.AddRange((await _dbContext.Cartons
                .Where(c => cartonIds.Contains(c.CartonId))
                .Select(c => c.CartonId)
                .ToListAsync()).Select(id => $"Carton {id}"));

            existing.AddRange((await _dbContext.Products
                .Where(p => productIds.Contains(p.RefId))
                .Select(p => p.RefId)
                .ToListAsync()).Select(id => $"Produit {id}"));

            return existing;
        }

        public async Task AddCommand(Command command)
        {
            await _dbContext.Commands.AddAsync(command);
            await _dbContext.SaveChangesAsync();
        }

        public async Task SaveChangeAsync()
        {
            await _dbContext.SaveChangesAsync();
        }

    }
}
