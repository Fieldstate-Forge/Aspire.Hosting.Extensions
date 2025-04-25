using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var registry = builder.AddAzureContainerRegistry("registry");
//.PublishAsExisting("mysampleacr", "rg-shared");

var cae = builder.AddAzureContainerAppEnvironment("cae").WithReference(registry);

builder.AddProject<Fieldstate_Testing_ApiService>("api").WithExternalHttpEndpoints();

builder.Build().Run();
