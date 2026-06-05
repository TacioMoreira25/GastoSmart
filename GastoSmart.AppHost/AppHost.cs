var builder = DistributedApplication.CreateBuilder(args);

var api = builder.AddProject<Projects.GastoSmart_Api>("api");

var publicDevTunnel = builder.AddDevTunnel("devtunnel-public")
    .WithAnonymousAccess()
    .WithReference(api.GetEndpoint("http"));

builder.Build().Run();