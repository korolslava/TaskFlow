using Dapper;
using MediatR;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace TaskFlow.Application.Sprints.Queries;

public record GetBurnDownQuery(Guid SprintId) : IRequest<BurnDownDto?>;

public record BurnDownDto(
    Guid SprintId,
    string SprintName,
    DateTime StartDate,
    DateTime EndDate,
    int TotalPoints,
    List<BurnDownDayDto> Days);

public record BurnDownDayDto(DateTime Date, int RemainingPoints, int IdealPoints);

public class GetBurnDownQueryHandler(IConfiguration config)
    : IRequestHandler<GetBurnDownQuery, BurnDownDto?>
{
    public async Task<BurnDownDto?> Handle(
        GetBurnDownQuery request, CancellationToken ct)
    {
        const string sprintSql = """
            SELECT s.Id, s.Name, s.StartDate, s.EndDate,
                   ISNULL(SUM(t.StoryPoints), 0) AS TotalPoints
            FROM Sprints s
            LEFT JOIN Tasks t ON t.SprintId = s.Id
            WHERE s.Id = @SprintId
            GROUP BY s.Id, s.Name, s.StartDate, s.EndDate
            """;

        await using var connection = new SqlConnection(
            config.GetConnectionString("DefaultConnection"));

        var sprint = await connection.QueryFirstOrDefaultAsync<dynamic>(
            sprintSql, new { request.SprintId });

        if (sprint is null || sprint.StartDate is null) return null;

        var startDate = (DateTime)sprint.StartDate;
        var endDate = (DateTime)sprint.EndDate;
        var totalPoints = (int)sprint.TotalPoints;
        var days = new List<BurnDownDayDto>();

        var totalDays = (endDate - startDate).Days;
        if (totalDays == 0) totalDays = 1;

        for (var d = startDate.Date; d <= endDate.Date; d = d.AddDays(1))
        {
            const string completedSql = """
                SELECT ISNULL(SUM(t.StoryPoints), 0)
                FROM Tasks t
                JOIN Columns c ON c.Id = t.ColumnId
                WHERE t.SprintId = @SprintId
                  AND c.Name = 'Done'
                """;

            var completedPoints = await connection.QueryFirstAsync<int>(
                completedSql, new { request.SprintId });

            var dayIndex = (d - startDate.Date).Days;
            var idealPoints = totalPoints -
                (int)Math.Round((double)totalPoints * dayIndex / totalDays);

            days.Add(new BurnDownDayDto(
                d,
                totalPoints - completedPoints,
                idealPoints));
        }

        return new BurnDownDto(
            (Guid)sprint.Id,
            (string)sprint.Name,
            startDate,
            endDate,
            totalPoints,
            days);
    }
}