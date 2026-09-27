using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Reception.Domain.Models;

namespace Reception.Infrastructure.Persistence.Configurations;

public class PaletteConfiguration : IEntityTypeConfiguration<Palette>
{
    public void Configure(EntityTypeBuilder<Palette> builder)
    {
        builder.HasKey(palette => palette.PaletteId);
        builder.Property(palette => palette.PaletteId).ValueGeneratedNever();

        builder.HasMany(palette => palette.Cartons)
            .WithOne(carton => carton.Palette)
            .HasForeignKey(carton => carton.PaletteId);
    }
}