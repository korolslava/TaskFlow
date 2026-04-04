using FluentAssertions;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace TaskFlow.IntegrationTests.Workspaces;

public class WorkspacesApiTests(CustomWebApplicationFactory factory)
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private async Task<string> GetTokenAsync(string email)
    {
        await _client.PostAsJsonAsync("/auth/register", new
        {
            email,
            displayName = "Test User",
            password = "Password123!"
        });

        var loginResponse = await _client.PostAsJsonAsync("/auth/login", new
        {
            email,
            password = "Password123!"
        });

        var body = await loginResponse.Content
            .ReadFromJsonAsync<Dictionary<string, string>>();

        return body!["accessToken"];
    }

    [Fact]
    public async Task CreateWorkspace_WithoutToken_Returns401()
    {
        var response = await _client.PostAsJsonAsync("/workspaces", new
        {
            name = "Test Workspace"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateWorkspace_ValidRequest_Returns201()
    {
        var token = await GetTokenAsync("workspace@test.com");
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.PostAsJsonAsync("/workspaces", new
        {
            name = "My Workspace"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await response.Content
            .ReadFromJsonAsync<JsonElement>();

        body.GetProperty("name").GetString().Should().Be("My Workspace");
        body.GetProperty("slug").GetString().Should().Be("my-workspace");
    }

    [Fact]
    public async Task GetWorkspace_AsNonMember_ReturnsNotFound()
    {
        var token1 = await GetTokenAsync("owner@test.com");
        var token2 = await GetTokenAsync("nonmember@test.com");

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token1);

        var createResponse = await _client.PostAsJsonAsync("/workspaces", new
        {
            name = "Private Workspace"
        });

        var workspace = await createResponse.Content
            .ReadFromJsonAsync<JsonElement>();
        var workspaceId = workspace.GetProperty("id").GetString();

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token2);

        var response = await _client.GetAsync($"/workspaces/{workspaceId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}