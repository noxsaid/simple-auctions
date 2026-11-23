using MySqlConnector;
using SimpleAuctions.Models;

namespace SimpleAuctions.Repositories;

public class UserRepository(IConfiguration configuration)
{
    private readonly string connectionString = configuration.GetConnectionString("AuctionDatabase")
        ?? throw new InvalidOperationException("Missing ConnectionStrings:AuctionDatabase.");

    public async Task<User?> GetByEmail(string email)
    {
        await using var connection = new MySqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = new MySqlCommand(
            "SELECT id, email, password_hash FROM users WHERE email = @email;", connection);
        command.Parameters.AddWithValue("@email", email);
        await using var reader = await command.ExecuteReaderAsync();
        return await reader.ReadAsync()
            ? new User(reader.GetInt32("id"), reader.GetString("email"), reader.GetString("password_hash"))
            : null;
    }

    public async Task<User?> GetById(int id)
    {
        await using var connection = new MySqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = new MySqlCommand(
            "SELECT id, email, password_hash FROM users WHERE id = @id;", connection);
        command.Parameters.AddWithValue("@id", id);
        await using var reader = await command.ExecuteReaderAsync();
        return await reader.ReadAsync()
            ? new User(reader.GetInt32("id"), reader.GetString("email"), reader.GetString("password_hash"))
            : null;
    }

    public async Task<User> Create(string email, string passwordHash)
    {
        await using var connection = new MySqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = new MySqlCommand(
            "INSERT INTO users (email, password_hash) VALUES (@email, @password);", connection);
        command.Parameters.AddWithValue("@email", email);
        command.Parameters.AddWithValue("@password", passwordHash);
        await command.ExecuteNonQueryAsync();
        return new User(checked((int)command.LastInsertedId), email, passwordHash);
    }
}
