namespace SimpleAuctions.DTOs;

public record CreateAuctionDto(string Name, DateTimeOffset EndTime);
