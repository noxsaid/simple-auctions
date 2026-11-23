using Microsoft.AspNetCore.Identity;
using SimpleAuctions.DTOs;
using SimpleAuctions.Models;
using SimpleAuctions.Repositories;

namespace SimpleAuctions.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/auth/login");
        group.MapPost("", async (LoginDto data, HttpContext context, UserRepository repository,
            IPasswordHasher<User> hasher) =>
        {
            if (string.IsNullOrWhiteSpace(data.Email) || string.IsNullOrEmpty(data.Password)
                || data.Email.Length > 254 || data.Password.Length > 128)
                return Results.BadRequest(new { message = "Email and password are required." });
            var user = await repository.GetByEmail(data.Email.Trim().ToLowerInvariant());
            if (user is null || hasher.VerifyHashedPassword(user, user.PasswordHash, data.Password)
                == PasswordVerificationResult.Failed)
                return Results.Unauthorized();
            context.Session.Clear();
            context.Session.SetInt32("UserId", user.Id);
            return Results.Ok(new { user = new UserResponseDto(user.Id, user.Email) });
        });
        group.MapGet("", async (HttpContext context, UserRepository repository) =>
        {
            int? id = context.Session.GetInt32("UserId");
            if (id is null) return Results.Unauthorized();
            var user = await repository.GetById(id.Value);
            return user is null ? Results.Unauthorized()
                : Results.Ok(new { authenticated = true, user = new UserResponseDto(user.Id, user.Email) });
        });
        group.MapDelete("", (HttpContext context) =>
        {
            context.Session.Clear();
            context.Response.Cookies.Delete("SimpleAuctions.Session");
            return Results.NoContent();
        });
    }
}
