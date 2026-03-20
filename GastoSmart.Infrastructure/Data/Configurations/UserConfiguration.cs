using GastoSmart.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GastoSmart.Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.SupabaseId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.Name)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(u => u.AlertThresholdPercentage)
            .HasPrecision(18, 2);

        builder.HasIndex(u => u.SupabaseId).IsUnique();
        builder.HasIndex(u => u.Email).IsUnique();
    }
}
