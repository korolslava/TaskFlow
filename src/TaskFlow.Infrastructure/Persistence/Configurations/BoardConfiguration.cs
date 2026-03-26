using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Infrastructure.Persistence.Configurations;

public class BoardConfiguration : IEntityTypeConfiguration<Board>
{
    public void Configure(EntityTypeBuilder<Board> builder)
    {
        builder.HasKey(b => b.Id);
        builder.Property(b => b.Name).IsRequired().HasMaxLength(100);

        builder.HasMany(b => b.Columns)
               .WithOne()
               .HasForeignKey(c => c.BoardId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}