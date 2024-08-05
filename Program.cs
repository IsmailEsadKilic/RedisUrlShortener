using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Redurl.Services;
using Redurl.Models;
using StackExchange.Redis;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Register RedisService
builder.Services.AddDbContext<DataContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("SQLServer")), contextLifetime: ServiceLifetime.Singleton);
builder.Services.AddSingleton<EFService>();
builder.Services.AddSingleton<RedisService>(sp => new RedisService(builder.Configuration.GetConnectionString("Redis"), sp.GetRequiredService<EFService>()));
// Add services to the container
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

/////////////////////////////////////////// url shortener //////////////////////////////////

app.MapPost("shorten", async (string url, RedisService redisService) =>
{
    if (string.IsNullOrEmpty(url))
    {
        return Results.BadRequest("URL cannot be empty.");
    }

    return Results.Ok(await redisService.Shorten(url));
});

app.MapGet("/{shortenedUrl}", async (string shortenedUrl, RedisService redisService) =>
{
    var originalUrl = await redisService.GetUrl(shortenedUrl);
    if (string.IsNullOrEmpty(originalUrl))
    {
        return Results.NotFound("URL not found. " + shortenedUrl);
    }

    return Results.Ok(originalUrl);
    // return Results.Redirect(originalUrl);
});

app.MapGet("/list", async (RedisService redisService) =>
{
    return Results.Ok(await redisService.Ls());
    // return "arda";
});

app.Run();

// app.MapPost("/publish", async (string channel, string message, RedisService redisService) =>
// {
//     redisService.Publish(channel, message);
//     return Results.Ok("Message published.");
// });

// app.MapGet("/subscribe/{channel}", async (string channel, RedisService redisService) =>
// {
//     var subscriber = redisService.GetSubscriber();
//     var tcs = new TaskCompletionSource<string>();

//     void MessageReceived(RedisChannel channel, RedisValue message)
//     {
//         tcs.SetResult(message);
//     }

//     subscriber.Subscribe(channel, MessageReceived);

//     var messageReceived = await tcs.Task;
//     subscriber.Unsubscribe(channel);
//     return Results.Ok(new { Channel = channel, Message = messageReceived });
// });