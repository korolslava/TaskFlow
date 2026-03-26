using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Infrastructure.Persistence.Configurations;

public class BoardColumnConfiguration : IEntityTypeConfiguration<BoardColumn>
{
    public void Configure(EntityTypeBuilder<BoardColumn> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Name).IsRequired().HasMaxLength(100);

        builder.HasIndex(c => new { c.BoardId, c.Order });

        builder.HasMany(c => c.Tasks)
               .WithOne()
               .HasForeignKey(t => t.ColumnId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}