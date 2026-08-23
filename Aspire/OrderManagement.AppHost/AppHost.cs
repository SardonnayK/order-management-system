var builder = DistributedApplication.CreateBuilder(args);

var api = builder.AddProject<Projects.OrderManagement_Api>("api");

builder.AddJavaScriptApp("frontend", "../../Frontend", "start")
    .WithHttpEndpoint(env: "PORT")
    .WithEnvironment("API_URL", api.GetEndpoint("http"))
    .WithExternalHttpEndpoints()
    .WaitFor(api);

builder.Build().Run();
