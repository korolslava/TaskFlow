using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Moq;
using TaskFlow.Application.Tasks.Commands;
using TaskFlow.Domain.Entities;
using TaskFlow.Infrastructure.Persistence;
using Xunit;

namespace TaskFlow.UnitTests.Tasks;

public class MoveTaskHandlerTests
{
    private static AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task Handle_ValidCommand_TaskMovedToNewColumn()
    {
        var db = CreateDb();
        var mediatorMock = new Mock<IMediator>();

        var fromColumn = BoardColumn.Create(Guid.NewGuid(), "To Do", 1);
        var toColumn = BoardColumn.Create(Guid.NewGuid(), "In Progress", 2);
        var task = TaskItem.Create(fromColumn.Id, "Test Task", 0);

        db.Columns.AddRange(fromColumn, toColumn);
        db.Tasks.Add(task);
        await db.SaveChangesAsync();

        var handler = new MoveTaskCommandHandler(db, mediatorMock.Object);
        var command = new MoveTaskCommand(
            task.Id, toColumn.Id, 0, Guid.NewGuid(), Guid.NewGuid());

        await handler.Handle(command, CancellationToken.None);

        var updatedTask = await db.Tasks.FirstAsync(t => t.Id == task.Id);
        updatedTask.ColumnId.Should().Be(toColumn.Id);
        updatedTask.Order.Should().Be(0);
    }

    [Fact]
    public async Task Handle_ValidCommand_PublishesDomainEvent()
    {
        var db = CreateDb();
        var mediatorMock = new Mock<IMediator>();

        var column = BoardColumn.Create(Guid.NewGuid(), "To Do", 1);
        var toColumn = BoardColumn.Create(Guid.NewGuid(), "Done", 2);
        var task = TaskItem.Create(column.Id, "Test Task", 0);

        db.Columns.AddRange(column, toColumn);
        db.Tasks.Add(task);
        await db.SaveChangesAsync();

        var handler = new MoveTaskCommandHandler(db, mediatorMock.Object);
        var boardId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var command = new MoveTaskCommand(
            task.Id, toColumn.Id, 0, boardId, userId);

        await handler.Handle(command, CancellationToken.None);

        mediatorMock.Verify(
            m => m.Publish(
                It.Is<Domain.Events.TaskMovedEvent>(e =>
                    e.TaskItemId == task.Id &&
                    e.BoardId == boardId),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_TaskNotFound_ThrowsException()
    {
        var db = CreateDb();
        var mediatorMock = new Mock<IMediator>();
        var handler = new MoveTaskCommandHandler(db, mediatorMock.Object);

        var command = new MoveTaskCommand(
            Guid.NewGuid(), Guid.NewGuid(), 0, Guid.NewGuid(), Guid.NewGuid());

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.Handle(command, CancellationToken.None));
    }
}