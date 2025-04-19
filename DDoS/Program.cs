﻿using NBomber.CSharp;
using NBomber.Http;

using var httpClient = new HttpClient();

var getScenario = Scenario.Create("ddos", async context =>
    {
        var response = await httpClient.PostAsync("https://localhost:7280/ddos/bad?id=1", null);

        return response.IsSuccessStatusCode ? Response.Ok() : Response.Fail();
    })
    .WithoutWarmUp()
    .WithLoadSimulations(
        Simulation.Inject(rate: 100,
            interval: TimeSpan.FromSeconds(1),
            during: TimeSpan.FromSeconds(300))
    );

NBomberRunner
    .RegisterScenarios(getScenario)
    .WithWorkerPlugins(new HttpMetricsPlugin())
    .Run();