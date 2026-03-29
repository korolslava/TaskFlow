using Dapper;
using MediatR;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace TaskFlow.Application.Projects.Queries;

public record GetProjectsQuery(Guid WorkspaceId)
    : IRequest<List<ProjectDto>>;

public record ProjectDto(
    Guid Id,
    string Name,
    string? Description,
    string Status,
    DateTime CreatedAt);

public class GetProjectsQueryHandler(IConfiguration config)
    : IRequestHandler<GetProjectsQuery, List<ProjectDto>>
{
    public async Task<List<ProjectDto>> Handle(
        GetProjectsQuery request, CancellationToken ct)
    {
        const string sql = """
            SELECT Id, Name, Description,
                   CASE Status
                       WHEN 0 THEN 'Active'
                       WHEN 1 THEN 'OnHold'
                       WHEN 2 THEN 'Completed'
                       WHEN 3 THEN 'Archived'
                   END AS Status,
                   CreatedAt
            FROM Projects
            WHERE WorkspaceId = @WorkspaceId
            ORDER BY CreatedAt DESC
            """;

        await using var connection = new SqlConnection(
            config.GetConnectionString("DefaultConnection"));

        var results = await connection.QueryAsync<ProjectDto>(
            sql, new { request.WorkspaceId });

        return results.ToList();
    }
}