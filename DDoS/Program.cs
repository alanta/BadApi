using NBomber.CSharp;
using NBomber.Http;

var endpoint = args.FirstOrDefault()?.Trim().ToLowerInvariant()
    ?? Environment.GetEnvironmentVariable("DDoS_ENDPOINT")
    ?? Environment.GetEnvironmentVariable("ddos-endpoint")
    ?? "bad";

if (endpoint is not "bad" and not "better" and not "good")
{
    Console.Error.WriteLine("Usage: dotnet run --project DDoS/DDoS.csproj -- [bad|better|good]");
    Console.Error.WriteLine("Or set DDoS_ENDPOINT=bad|better|good");
    return;
}

var targetUrl = $"https://localhost:7280/ddos/{endpoint}?id=1";
Console.WriteLine($"Starting DDoS load test against: {targetUrl}");

using var httpClient = new HttpClient();

var getScenario = Scenario.Create("ddos", async context =>
    {
        var response = await httpClient.PostAsync(targetUrl, null);

        if (!response.IsSuccessStatusCode)
        {
            return Response.Fail(statusCode: ((int)response.StatusCode).ToString());
        }

        var sizeBytes = response.Content.Headers.ContentLength ?? 0;
        return Response.Ok(statusCode: ((int)response.StatusCode).ToString(), sizeBytes: sizeBytes);
    })
    .WithoutWarmUp()
    .WithLoadSimulations(
        Simulation.Inject(rate: 20,
            interval: TimeSpan.FromSeconds(1),
            during: TimeSpan.FromSeconds(300))
    );

NBomberRunner
    .RegisterScenarios(getScenario)
    .WithWorkerPlugins(new HttpMetricsPlugin())
    .Run();