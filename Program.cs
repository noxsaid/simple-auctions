using Microsoft.AspNetCore.Identity;
using SimpleAuctions.Endpoints;
using SimpleAuctions.Models;
using SimpleAuctions.Repositories;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<AuctionRepository>();
builder.Services.AddScoped<BidRepository>();
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.Name = "SimpleAuctions.Session";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;
    options.IdleTimeout = TimeSpan.FromMinutes(30);
});
builder.Services.AddProblemDetails();
var app = builder.Build();
app.UseExceptionHandler();
app.UseSession();
app.MapGet("/", () => Results.Ok(new { message = "Simple Auctions API" }));
app.MapUserEndpoints();
app.MapAuthEndpoints();
app.MapAuctionEndpoints();
app.MapBidEndpoints();
app.Run();
