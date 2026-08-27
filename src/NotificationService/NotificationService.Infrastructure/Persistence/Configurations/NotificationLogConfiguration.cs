using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NotificationService.Domain.Entities;

namespace NotificationService.Infrastructure.Persistence.Configurations;

public sealed class NotificationLogConfiguration : IEntityTypeConfiguration<NotificationLog>
{
    public void Configure(EntityTypeBuilder<NotificationLog> builder)
    {
        builder.ToTable("notification_logs");
        builder.HasKey(n => n.Id);

        // Índice único: es la garantía real de idempotencia a nivel de base de datos,
        // no solo una verificación en memoria antes del insert.
        builder.HasIndex(n => n.MessageId).IsUnique();

        builder.Property(n => n.EventName).HasMaxLength(200).IsRequired();
        builder.Property(n => n.PayloadHash).HasMaxLength(64).IsRequired();
        builder.Property(n => n.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(n => n.ErrorMessage).HasMaxLength(1000);
    }
}
