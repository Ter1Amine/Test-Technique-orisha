using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Reception.Domain.Models;

namespace Reception.Infrastructure.Persistence.Configurations;

public class CartonConfiguration : IEntityTypeConfiguration<Carton>
{
    public void Configure(EntityTypeBuilder<Carton> builder)
    {
        builder.HasKey(carton => carton.CartonId);
        builder.Property(carton => carton.CartonId).ValueGeneratedNever();

        builder.HasMany(carton => carton.Products)
            .WithOne(product => product.Carton)
            .HasForeignKey(product => product.CartonId);
    }
}