using SimpleAuctions.DTOs;
using SimpleAuctions.Repositories;

namespace SimpleAuctions.Endpoints;

public static class AuctionEndpoints
{
    public static void MapAuctionEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/auctions");
        group.MapGet("", async (AuctionRepository repository) => Results.Ok(await repository.GetAll()));
        group.MapGet("/{id:int}", async (int id, AuctionRepository auctions, BidRepository bids) =>
        {
            var auction = await auctions.GetById(id);
            if (auction is null) return Results.NotFound();
            var history = await bids.GetByAuction(id);
            bool ended = DateTimeOffset.UtcNow >= auction.EndTime;
            var highest = history.FirstOrDefault();
            return Results.Ok(new
            {
                auction.Id, auction.Name, auction.EndTime, auction.SellerId,
                ended, currentPrice = highest?.Amount,
                winnerId = ended ? highest?.BuyerId : null,
                bids = history
            });
        });
        group.MapPost("", async (CreateAuctionDto data, HttpContext context, AuctionRepository repository) =>
        {
            int? sellerId = context.Session.GetInt32("UserId");
            if (sellerId is null) return Results.Unauthorized();
            if (string.IsNullOrWhiteSpace(data.Name) || data.Name.Trim().Length > 200
                || data.EndTime <= DateTimeOffset.UtcNow || data.EndTime.Year < 1000)
                return Results.BadRequest(new { message = "Name (1–200 characters) and a future endTime are required." });
            var auction = await repository.Create(data.Name.Trim(), data.EndTime, sellerId.Value);
            return Results.Created($"/api/auctions/{auction.Id}", auction);
        });
    }
}
