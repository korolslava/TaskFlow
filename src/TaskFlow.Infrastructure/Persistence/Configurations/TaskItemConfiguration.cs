using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Infrastructure.Persistence.Configurations;

public class TaskItemConfiguration : IEntityTypeConfiguration<TaskItem>
{
    public void Configure(EntityTypeBuilder<TaskItem> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Title).IsRequired().HasMaxLength(200);
        builder.Property(t => t.Description).HasMaxLength(2000);
        builder.Property(t => t.Priority).HasConversion<int>();

        builder.HasIndex(t => new { t.ColumnId, t.Order });

        builder.HasMany(t => t.Comments)
               .WithOne()
               .HasForeignKey(c => c.TaskItemId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
