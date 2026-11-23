namespace SimpleAuctions.Models;

public record Auction(int Id, string Name, DateTimeOffset EndTime, int SellerId);
