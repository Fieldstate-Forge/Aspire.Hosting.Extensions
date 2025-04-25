using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Azure;
using Aspire.Hosting.Lifecycle;
using Azure.Provisioning;
using Azure.Provisioning.ContainerRegistry;

namespace Aspire.Hosting;

public static class AzureContainerRegistryApplicationBuilderExtensions
{
    public static IResourceBuilder<AzureContainerRegistryResource> AddAzureContainerRegistry(this IDistributedApplicationBuilder builder, string name)
    {
        builder.Services.TryAddLifecycleHook<RegistryOverrideLifecycleHook>();
        var registry = new AzureContainerRegistryResource(name, static infrastructure =>
        {
            var registryResource = (AzureContainerRegistryResource)infrastructure.AspireResource;
            var containerRegistry = new ContainerRegistryService(registryResource.GetBicepIdentifier())
            {
                Sku = new() { Name = ContainerRegistrySkuName.Basic },
            };
            infrastructure.Add(containerRegistry);
            infrastructure.Add(new ProvisioningOutput(AzureContainerConstants.AZURE_CONTAINER_REGISTRY_NAME, typeof(string))
            {
                Value = containerRegistry.Name
            });
            infrastructure.Add(new ProvisioningOutput(AzureContainerConstants.AZURE_CONTAINER_REGISTRY_ENDPOINT, typeof(string))
            {
                Value = containerRegistry.LoginServer
            });
            infrastructure.Add(new ProvisioningOutput(AzureContainerConstants.AZURE_CONTAINER_REGISTRY_ID, typeof(string))
            {
                Value = containerRegistry.Id
            });
        });

        return builder.AddResource(registry);
    }
}