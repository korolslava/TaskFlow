using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace TaskFlow.IntegrationTests.Auth;

public class AuthApiTests(CustomWebApplicationFactory factory)
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Register_ValidRequest_Returns200()
    {
        var request = new
        {
            email = "integration@test.com",
            displayName = "Integration User",
            password = "Password123!"
        };

        var response = await _client.PostAsJsonAsync("/auth/register", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<dynamic>();
        body.Should().NotBeNull();
    }

    [Fact]
    public async Task Register_DuplicateEmail_Returns409()
    {
        var request = new
        {
            email = "duplicate@test.com",
            displayName = "Duplicate User",
            password = "Password123!"
        };

        await _client.PostAsJsonAsync("/auth/register", request);
        var response = await _client.PostAsJsonAsync("/auth/register", request);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsTokens()
    {
        var email = "login@test.com";
        var password = "Password123!";

        await _client.PostAsJsonAsync("/auth/register", new
        {
            email,
            displayName = "Login User",
            password
        });

        var response = await _client.PostAsJsonAsync("/auth/login", new
        {
            email,
            password
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content
            .ReadFromJsonAsync<Dictionary<string, string>>();

        body.Should().ContainKey("accessToken");
        body.Should().ContainKey("refreshToken");
        body!["accessToken"].Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_InvalidPassword_Returns401()
    {
        await _client.PostAsJsonAsync("/auth/register", new
        {
            email = "wrongpass@test.com",
            displayName = "Wrong Pass User",
            password = "Password123!"
        });

        var response = await _client.PostAsJsonAsync("/auth/login", new
        {
            email = "wrongpass@test.com",
            password = "WrongPassword!"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}