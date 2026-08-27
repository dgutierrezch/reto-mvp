using EventService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventService.Infrastructure.Persistence.Configurations;

public sealed class EventConfiguration : IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> builder)
    {
        builder.ToTable("events");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name).HasMaxLength(200).IsRequired();
        builder.Property(e => e.Location).HasMaxLength(300).IsRequired();
        builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(e => e.Date).IsRequired();
        builder.Property(e => e.CreatedAt).IsRequired();

        builder.HasMany(e => e.Zones)
            .WithOne()
            .HasForeignKey(z => z.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata.FindNavigation(nameof(Event.Zones))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
