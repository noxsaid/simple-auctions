using System.Net.Mail;
using Microsoft.AspNetCore.Identity;
using MySqlConnector;
using SimpleAuctions.DTOs;
using SimpleAuctions.Models;
using SimpleAuctions.Repositories;

namespace SimpleAuctions.Endpoints;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this WebApplication app)
    {
        app.MapPost("/api/users", async (CreateUserDto data, UserRepository repository,
            IPasswordHasher<User> hasher) =>
        {
            string email = data.Email?.Trim().ToLowerInvariant() ?? "";
            if (email.Length > 254 || !MailAddress.TryCreate(email, out var address) || address.Address != email
                || string.IsNullOrWhiteSpace(data.Password) || data.Password.Length > 128)
                return Results.BadRequest(new { message = "A valid email and password (1–128 characters) are required." });
            var user = new User(0, email, "");
            string hash = hasher.HashPassword(user, data.Password);
            try
            {
                user = await repository.Create(email, hash);
                return Results.Created($"/api/users/{user.Id}", new UserResponseDto(user.Id, user.Email));
            }
            catch (MySqlException ex) when (ex.Number == 1062)
            {
                return Results.Conflict(new { message = "Email is already registered." });
            }
        });
        // Endast egen användare; inga lösenordshashar i API-svaren.
        app.MapGet("/api/users/{id:int}", async (int id, HttpContext context, UserRepository repository) =>
        {
            int? userId = context.Session.GetInt32("UserId");
            if (userId is null) return Results.Unauthorized();
            if (id != userId) return Results.StatusCode(403);
            var user = await repository.GetById(id);
            return user is null ? Results.NotFound() : Results.Ok(new UserResponseDto(user.Id, user.Email));
        });
    }
}
