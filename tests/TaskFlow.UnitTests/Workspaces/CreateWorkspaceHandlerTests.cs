using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Workspaces.Commands;
using TaskFlow.Infrastructure.Persistence;
using Xunit;

namespace TaskFlow.UnitTests.Workspaces;

public class CreateWorkspaceHandlerTests
{
    private static AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task Handle_ValidCommand_CreatesWorkspace()
    {
        var db = CreateDb();
        var handler = new CreateWorkspaceCommandHandler(db);
        var command = new CreateWorkspaceCommand("Test Workspace", Guid.NewGuid());

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Name.Should().Be("Test Workspace");
        result.Slug.Should().Be("test-workspace");
    }

    [Fact]
    public async Task Handle_ValidCommand_OwnerAddedAsMember()
    {
        var db = CreateDb();
        var handler = new CreateWorkspaceCommandHandler(db);
        var ownerId = Guid.NewGuid();
        var command = new CreateWorkspaceCommand("My Workspace", ownerId);

        var result = await handler.Handle(command, CancellationToken.None);

        var workspace = await db.Workspaces
            .Include(w => w.Members)
            .FirstAsync(w => w.Id == result.Id);

        workspace.Members.Should().HaveCount(1);
        workspace.Members.First().UserId.Should().Be(ownerId);
        workspace.Members.First().Role.Should()
            .Be(WorkspaceRole.Owner);
    }

    [Fact]
    public async Task Handle_ValidCommand_WorkspaceSavedToDatabase()
    {
        var db = CreateDb();
        var handler = new CreateWorkspaceCommandHandler(db);
        var command = new CreateWorkspaceCommand("Saved Workspace", Guid.NewGuid());

        await handler.Handle(command, CancellationToken.None);

        var count = await db.Workspaces.CountAsync();
        count.Should().Be(1);
    }
}