var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.BadApi>("badapi");

builder.AddProject<Projects.DDoS>("ddos-bad")
    .WithArgs(["bad"])
    .WithExplicitStart();

builder.AddProject<Projects.DDoS>("ddos-better")
    .WithArgs(["better"])
    .WithExplicitStart();

builder.AddProject<Projects.DDoS>("ddos-good")
    .WithArgs(["good"])
    .WithExplicitStart();

builder.Build().Run();