using MySqlConnector;
using SimpleAuctions.Models;

namespace SimpleAuctions.Repositories;

public class AuctionRepository(IConfiguration configuration)
{
    private readonly string connectionString = configuration.GetConnectionString("AuctionDatabase")
        ?? throw new InvalidOperationException("Missing ConnectionStrings:AuctionDatabase.");

    public async Task<List<Auction>> GetAll()
    {
        await using var connection = new MySqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = new MySqlCommand(
            "SELECT id, name, end_time, seller_id FROM auctions ORDER BY id;", connection);
        await using var reader = await command.ExecuteReaderAsync();
        List<Auction> auctions = [];
        while (await reader.ReadAsync())
            auctions.Add(ReadAuction(reader));
        return auctions;
    }

    public async Task<Auction?> GetById(int id)
    {
        await using var connection = new MySqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = new MySqlCommand(
            "SELECT id, name, end_time, seller_id FROM auctions WHERE id = @id;", connection);
        command.Parameters.AddWithValue("@id", id);
        await using var reader = await command.ExecuteReaderAsync();
        return await reader.ReadAsync() ? ReadAuction(reader) : null;
    }

    public async Task<Auction> Create(string name, DateTimeOffset endTime, int sellerId)
    {
        await using var connection = new MySqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = new MySqlCommand("""
            INSERT INTO auctions (name, end_time, seller_id) VALUES (@name, @end, @seller);
            """, connection);
        command.Parameters.AddWithValue("@name", name);
        command.Parameters.AddWithValue("@end", endTime.UtcDateTime);
        command.Parameters.AddWithValue("@seller", sellerId);
        await command.ExecuteNonQueryAsync();
        return new Auction(checked((int)command.LastInsertedId), name, endTime.ToUniversalTime(), sellerId);
    }

    private static Auction ReadAuction(MySqlDataReader reader) => new(
        reader.GetInt32("id"), reader.GetString("name"),
        new DateTimeOffset(DateTime.SpecifyKind(reader.GetDateTime("end_time"), DateTimeKind.Utc)),
        reader.GetInt32("seller_id"));
}
