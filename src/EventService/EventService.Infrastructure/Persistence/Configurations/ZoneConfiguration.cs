using EventService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventService.Infrastructure.Persistence.Configurations;

public sealed class ZoneConfiguration : IEntityTypeConfiguration<Zone>
{
    public void Configure(EntityTypeBuilder<Zone> builder)
    {
        builder.ToTable("zones");
        builder.HasKey(z => z.Id);

        builder.Property(z => z.Name).HasMaxLength(100).IsRequired();
        builder.Property(z => z.Price).HasColumnType("numeric(10,2)").IsRequired();
        builder.Property(z => z.Capacity).IsRequired();
    }
}
