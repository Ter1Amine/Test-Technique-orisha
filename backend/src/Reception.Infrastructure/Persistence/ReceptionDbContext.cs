using Microsoft.EntityFrameworkCore;
using Reception.Domain.Models;
using Reception.Infrastructure.Persistence.Configurations;

namespace Reception.Infrastructure.Persistence;

public class ReceptionDbContext : DbContext
{
    public ReceptionDbContext(DbContextOptions<ReceptionDbContext> options) : base(options)
	{
	}

	public DbSet<Command> Commands => Set<Command>();
	public DbSet<Palette> Palettes => Set<Palette>();
	public DbSet<Carton> Cartons => Set<Carton>();
	public DbSet<Product> Products => Set<Product>();

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.ApplyConfiguration(new CommandConfiguration());
		modelBuilder.ApplyConfiguration(new PaletteConfiguration());
		modelBuilder.ApplyConfiguration(new CartonConfiguration());
		modelBuilder.ApplyConfiguration(new ProductConfiguration());
	}
}
