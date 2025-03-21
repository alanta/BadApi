using Microsoft.AspNetCore.RateLimiting;

namespace BadApi.DDoS;

public static class Endpoints
{
    public static async Task<IResult> Bad(string id)
    {
        // This is a slow, blocking call
        Thread.Sleep(TimeSpan.FromSeconds(5));

        return Results.Ok();
    }
    
    public static async Task<IResult> Better(string id)
    {
        // This is still a slow, blocking call, but rate limiting will prevent flooding
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