using Dapper;
using MediatR;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace TaskFlow.Application.Comments.Queries;

public record GetCommentsQuery(Guid TaskItemId) : IRequest<List<CommentDto>>;

public record CommentDto(
    Guid Id,
    Guid AuthorId,
    string AuthorName,
    string Content,
    DateTime CreatedAt);

public class GetCommentsQueryHandler(IConfiguration config)
    : IRequestHandler<GetCommentsQuery, List<CommentDto>>
{
    public async Task<List<CommentDto>> Handle(
        GetCommentsQuery request, CancellationToken ct)
    {
        const string sql = """
            SELECT c.Id, c.AuthorId,
                   u.DisplayName AS AuthorName,
                   c.Content, c.CreatedAt
            FROM Comments c
            JOIN Users u ON u.Id = c.AuthorId
            WHERE c.TaskItemId = @TaskItemId
            ORDER BY c.CreatedAt ASC
            """;

        await using var connection = new SqlConnection(
            config.GetConnectionString("DefaultConnection"));

        var results = await connection.QueryAsync<CommentDto>(
            sql, new { request.TaskItemId });

        return results.ToList();
    }
}