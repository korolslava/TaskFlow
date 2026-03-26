using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Infrastructure.Persistence.Configurations;

public class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Content).IsRequired().HasMaxLength(2000);

        builder.HasOne<User>()
               .WithMany()
               .HasForeignKey(c => c.AuthorId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}