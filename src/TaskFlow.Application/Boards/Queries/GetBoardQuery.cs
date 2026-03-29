using Dapper;
using MediatR;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace TaskFlow.Application.Boards.Queries;

public record GetBoardQuery(Guid BoardId) : IRequest<BoardDto?>;

public record BoardDto(
    Guid Id,
    string Name,
    Guid ProjectId,
    List<ColumnDto> Columns);

public record ColumnDto(
    Guid Id,
    string Name,
    int Order,
    List<TaskDto> Tasks);

public record TaskDto(
    Guid Id,
    string Title,
    string? Description,
    string Priority,
    int StoryPoints,
    int Order,
    Guid? AssigneeId,
    DateTime? DueDate);

public class GetBoardQueryHandler(IConfiguration config)
    : IRequestHandler<GetBoardQuery, BoardDto?>
{
    public async Task<BoardDto?> Handle(
        GetBoardQuery request, CancellationToken ct)
    {
        const string sql = """
            SELECT
                b.Id        AS BoardId,
                b.Name      AS BoardName,
                b.ProjectId AS BoardProjectId,
                c.Id        AS ColumnId,
                c.Name      AS ColumnName,
                c.[Order]   AS ColumnOrder,
                t.Id        AS TaskId,
                t.Title     AS TaskTitle,
                t.Description AS TaskDescription,
                CASE t.Priority
                    WHEN 0 THEN 'Low'
                    WHEN 1 THEN 'Medium'
                    WHEN 2 THEN 'High'
                    WHEN 3 THEN 'Urgent'
                END         AS TaskPriority,
                t.StoryPoints AS TaskStoryPoints,
                t.[Order]   AS TaskOrder,
                t.AssigneeId AS TaskAssigneeId,
                t.DueDate   AS TaskDueDate
            FROM Boards b
            LEFT JOIN Columns c ON c.BoardId = b.Id
            LEFT JOIN Tasks t   ON t.ColumnId = c.Id
            WHERE b.Id = @BoardId
            ORDER BY c.[Order], t.[Order]
            """;

        await using var connection = new SqlConnection(
            config.GetConnectionString("DefaultConnection"));

        BoardDto? board = null;
        var columnMap = new Dictionary<Guid, ColumnDto>();

        var rows = await connection.QueryAsync<dynamic>(sql, new { request.BoardId });

        foreach (var row in rows)
        {
            if (board is null)
                board = new BoardDto(
                    (Guid)row.BoardId,
                    (string)row.BoardName,
                    (Guid)row.BoardProjectId,
                    new List<ColumnDto>());

            if (row.ColumnId is not null)
            {
                var colId = (Guid)row.ColumnId;
                if (!columnMap.TryGetValue(colId, out var column))
                {
                    column = new ColumnDto(
                        colId,
                        (string)row.ColumnName,
                        (int)row.ColumnOrder,
                        new List<TaskDto>());
                    columnMap[colId] = column;
                    board.Columns.Add(column);
                }

                if (row.TaskId is not null)
                {
                    column.Tasks.Add(new TaskDto(
                        (Guid)row.TaskId,
                        (string)row.TaskTitle,
                        (string?)row.TaskDescription,
                        (string)row.TaskPriority,
                        (int)row.TaskStoryPoints,
                        (int)row.TaskOrder,
                        (Guid?)row.TaskAssigneeId,
                        (DateTime?)row.TaskDueDate));
                }
            }
        }
    
        return board;
    }
}