using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Reception.Domain.Models;

namespace Reception.Infrastructure.Persistence.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(product => product.RefId);
        builder.Property(product => product.RefId).ValueGeneratedNever();
        builder.Property(product => product.Name).IsRequired();
        builder.Property(product => product.Color).IsRequired();
        builder.Property(product => product.Size).IsRequired();
    }
}