using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Infrastructure.Persistence.Configurations;

public class WorkspaceMemberConfiguration : IEntityTypeConfiguration<WorkspaceMember>
{
    public void Configure(EntityTypeBuilder<WorkspaceMember> builder)
    {
        builder.HasKey(m => new { m.WorkspaceId, m.UserId });
        builder.Property(m => m.Role).HasConversion<int>();

        builder.HasOne(m => m.User)
               .WithMany()
               .HasForeignKey(m => m.UserId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}