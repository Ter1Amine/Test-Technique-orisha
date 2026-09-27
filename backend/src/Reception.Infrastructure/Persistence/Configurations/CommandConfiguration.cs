using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Reception.Domain.Models;

namespace Reception.Infrastructure.Persistence.Configurations;

public class CommandConfiguration : IEntityTypeConfiguration<Command>
{
    public void Configure(EntityTypeBuilder<Command> builder)
    {
        builder.HasKey(command => command.CommandId);
        builder.Property(command => command.CommandId).ValueGeneratedNever();

        builder.HasMany(command => command.Palettes)
            .WithOne(palette => palette.Command)
            .HasForeignKey(palette => palette.CommandId);
    }
}