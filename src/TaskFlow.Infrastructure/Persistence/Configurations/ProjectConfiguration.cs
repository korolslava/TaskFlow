using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Infrastructure.Persistence.Configurations;

public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Name).IsRequired().HasMaxLength(100);
        builder.Property(p => p.Description).HasMaxLength(500);
        builder.Property(p => p.Status).HasConversion<int>();

        builder.HasMany(p => p.Boards)
               .WithOne()
               .HasForeignKey(b => b.ProjectId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Sprints)
               .WithOne()
               .HasForeignKey(s => s.ProjectId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}