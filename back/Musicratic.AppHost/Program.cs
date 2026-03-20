var builder = DistributedApplication.CreateBuilder(args);

var api = builder.AddProject<Projects.Musicratic_Core>("musicratic-api");

builder.AddProject<Projects.Musicratic_Front_MVP>("musicratic-front")
    .WithEnvironment("ApiBaseUrl", api.GetEndpoint("http"))
    .WaitFor(api);

builder.Build().Run();
