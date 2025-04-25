using Aspire.Hosting.Azure;

namespace Aspire.Hosting;

public class AzureContainerRegistryResource(string name, Action<AzureResourceInfrastructure> configureInfrastructure) :
    AzureProvisioningResource(name, configureInfrastructure);
