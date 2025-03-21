var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.BadApi>("badapi");

builder.Build().Run();