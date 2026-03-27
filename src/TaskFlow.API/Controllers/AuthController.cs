using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Domain.Entities;
using TaskFlow.Infrastructure.Auth;
using TaskFlow.Infrastructure.Persistence;

namespace TaskFlow.API.Controllers;

[ApiController]
[Route("auth")]
public class AuthController(AppDbContext db, JwtTokenService jwt) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest req)
    {
        if (await db.Users.AnyAsync(u => u.Email == req.Email.ToLower()))
            return Conflict("Email already exists.");

        var hash = BCrypt.Net.BCrypt.HashPassword(req.Password);
        var user = TaskFlow.Domain.Entities.User.Create(req.Email, req.DisplayName, hash);
        db.Users.Add(user);
        await db.SaveChangesAsync();

        return Ok(new { user.Id, user.Email, user.DisplayName });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest req)
    {
        var user = await db.Users
            .FirstOrDefaultAsync(u => u.Email == req.Email.ToLower());

        if (user is null || !BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
            return Unauthorized("Invalid credentials.");

        var refreshToken = jwt.GenerateRefreshToken();
        user.SetRefreshToken(refreshToken, DateTime.UtcNow.AddDays(7));
        await db.SaveChangesAsync();

        return Ok(new
        {
            AccessToken = jwt.GenerateAccessToken(user),
            RefreshToken = refreshToken
        });
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(RefreshRequest req)
    {
        var user = await db.Users
            .FirstOrDefaultAsync(u => u.RefreshToken == req.RefreshToken);

        if (user is null || user.RefreshTokenExpiry < DateTime.UtcNow)
            return Unauthorized("Invalid or expired refresh token.");

        var newRefresh = jwt.GenerateRefreshToken();
        user.SetRefreshToken(newRefresh, DateTime.UtcNow.AddDays(7));
        await db.SaveChangesAsync();

        return Ok(new
        {
            AccessToken = jwt.GenerateAccessToken(user),
            RefreshToken = newRefresh
        });
    }
}

public record RegisterRequest(string Email, string DisplayName, string Password);
public record LoginRequest(string Email, string Password);
public record RefreshRequest(string RefreshToken);