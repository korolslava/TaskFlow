using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Infrastructure.Persistence.Configurations;

public class SprintConfiguration : IEntityTypeConfiguration<Sprint>
{
    public void Configure(EntityTypeBuilder<Sprint> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Name).IsRequired().HasMaxLength(100);
        builder.Property(s => s.Goal).HasMaxLength(500);
        builder.Property(s => s.Status).HasConversion<int>();

        builder.HasMany(s => s.Tasks)
               .WithOne()
               .HasForeignKey(t => t.SprintId)
               .OnDelete(DeleteBehavior.NoAction);
    }
}