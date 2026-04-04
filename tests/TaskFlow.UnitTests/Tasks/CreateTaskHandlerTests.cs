using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Moq;
using TaskFlow.Application.Tasks.Commands;
using TaskFlow.Domain.Entities;
using TaskFlow.Infrastructure.Persistence;
using Xunit;

namespace TaskFlow.UnitTests.Tasks;

public class CreateTaskHandlerTests
{
    private static AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task Handle_ValidCommand_CreatesTask()
    {
        var db = CreateDb();
        var mediatorMock = new Mock<IMediator>();
        var column = BoardColumn.Create(Guid.NewGuid(), "To Do", 1);
        db.Columns.Add(column);
        await db.SaveChangesAsync();

        var handler = new CreateTaskCommandHandler(db, mediatorMock.Object);
        var command = new CreateTaskCommand(
            column.Id, Guid.NewGuid(), Guid.NewGuid(),
            "New Task", null, TaskPriority.High, 5);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Title.Should().Be("New Task");
        result.Order.Should().Be(0);
    }

    [Fact]
    public async Task Handle_MultipleTasksInColumn_OrderIncrements()
    {
        var db = CreateDb();
        var mediatorMock = new Mock<IMediator>();
        var column = BoardColumn.Create(Guid.NewGuid(), "To Do", 1);
        db.Columns.Add(column);

        var existingTask = TaskItem.Create(column.Id, "Existing Task", 0);
        db.Tasks.Add(existingTask);
        await db.SaveChangesAsync();

        var handler = new CreateTaskCommandHandler(db, mediatorMock.Object);
        var command = new CreateTaskCommand(
            column.Id, Guid.NewGuid(), Guid.NewGuid(),
            "New Task", null, TaskPriority.Medium, 3);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Order.Should().Be(1);
    }

    [Fact]
    public async Task Handle_ColumnNotFound_ThrowsException()
    {
        var db = CreateDb();
        var mediatorMock = new Mock<IMediator>();
        var handler = new CreateTaskCommandHandler(db, mediatorMock.Object);
        var command = new CreateTaskCommand(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            "Task", null, TaskPriority.Low, 0);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.Handle(command, CancellationToken.None));
    }
}