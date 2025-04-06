using System.Net.Sockets;

namespace BadApi.DDoS;

public static class Endpoints
{
    public static void Register(WebApplication app)
    {
        app.MapPost("/ddos/bad", (Delegate)BadApi.DDoS.Endpoints.Bad);
        app.MapPost("/ddos/better", (Delegate)BadApi.DDoS.Endpoints.Better).RequireRateLimiting("fixed");
        app.MapPost("/ddos/good", (Delegate)BadApi.DDoS.Endpoints.Good).RequireRateLimiting("fixed");
    } 
    
    public static async Task<IResult> Bad(string id)
    {
        // This is a slow, blocking call
        Thread.Sleep(TimeSpan.FromSeconds(5));

        return Results.Ok();
    }
    
    public static async Task<IResult> Better(string id)
    {
        // This is still a slow, blocking call, even rate limiting will not prevent the app from becoming unresponsive
        Thread.Sleep(TimeSpan.FromSeconds(5));

        return Results.Ok();
    }
    
    public static async Task<IResult> Good(string id)
    {
        // This is still a slow call, but it's non-blocking
        await Task.Delay(TimeSpan.FromSeconds(5));

        return Results.Ok();
    }
}