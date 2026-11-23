using SimpleAuctions.DTOs;
using SimpleAuctions.Repositories;

namespace SimpleAuctions.Endpoints;

public static class BidEndpoints
{
    public static void MapBidEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/auctions/{id:int}/bids");
        group.MapGet("", async (int id, AuctionRepository auctions, BidRepository bids) =>
        {
            if (await auctions.GetById(id) is null) return Results.NotFound();
            return Results.Ok(await bids.GetByAuction(id));
        });
        group.MapPost("", async (int id, CreateBidDto data, HttpContext context, BidRepository repository) =>
        {
            int? buyerId = context.Session.GetInt32("UserId");
            if (buyerId is null) return Results.Unauthorized();
            var result = await repository.Create(id, data.Amount, buyerId.Value);
            if (!result.Found) return Results.NotFound();
            if (result.Error is not null) return Results.BadRequest(new { message = result.Error });
            return Results.Created($"/api/auctions/{id}/bids", result.Bid);
        });
    }
}
