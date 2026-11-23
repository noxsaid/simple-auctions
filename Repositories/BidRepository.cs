using MySqlConnector;
using SimpleAuctions.Models;
using SimpleAuctions.Rules;

namespace SimpleAuctions.Repositories;

public class BidRepository(IConfiguration configuration)
{
    private readonly string connectionString = configuration.GetConnectionString("AuctionDatabase")
        ?? throw new InvalidOperationException("Missing ConnectionStrings:AuctionDatabase.");

    public async Task<List<Bid>> GetByAuction(int auctionId)
    {
        await using var connection = new MySqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = new MySqlCommand("""
            SELECT id, amount, time, buyer_id, auction_id FROM bids
            WHERE auction_id = @auction ORDER BY amount DESC, id;
            """, connection);
        command.Parameters.AddWithValue("@auction", auctionId);
        await using var reader = await command.ExecuteReaderAsync();
        List<Bid> bids = [];
        while (await reader.ReadAsync())
            bids.Add(new Bid(reader.GetInt32("id"), reader.GetDecimal("amount"),
                new DateTimeOffset(DateTime.SpecifyKind(reader.GetDateTime("time"), DateTimeKind.Utc)),
                reader.GetInt32("buyer_id"), reader.GetInt32("auction_id")));
        return bids;
    }

    public async Task<(Bid? Bid, string? Error, bool Found)> Create(int auctionId, decimal amount, int buyerId)
    {
        await using var connection = new MySqlConnection(connectionString);
        await connection.OpenAsync();
        await using (var command = new MySqlCommand(
            "SELECT id FROM auctions WHERE id = @id;", connection))
        {
            command.Parameters.AddWithValue("@id", auctionId);
            if (await command.ExecuteScalarAsync() is null)
                return (null, "Auction not found.", false);
        }

        string? error = BidRules.Validate(amount);
        if (error is not null)
            return (null, error, true);

        var now = DateTimeOffset.UtcNow;
        await using var insert = new MySqlCommand("""
            INSERT INTO bids (amount, time, buyer_id, auction_id)
            VALUES (@amount, @time, @buyer, @auction);
            """, connection);
        insert.Parameters.AddWithValue("@amount", amount);
        insert.Parameters.AddWithValue("@time", now.UtcDateTime);
        insert.Parameters.AddWithValue("@buyer", buyerId);
        insert.Parameters.AddWithValue("@auction", auctionId);
        await insert.ExecuteNonQueryAsync();
        var bid = new Bid(checked((int)insert.LastInsertedId), amount, now, buyerId, auctionId);
        return (bid, null, true);
    }
}
