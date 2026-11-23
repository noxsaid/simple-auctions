namespace SimpleAuctions.Models;

public record Bid(int Id, decimal Amount, DateTimeOffset Time, int BuyerId, int AuctionId);
