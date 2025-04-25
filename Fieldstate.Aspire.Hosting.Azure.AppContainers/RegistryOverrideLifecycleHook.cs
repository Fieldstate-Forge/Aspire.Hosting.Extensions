using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Azure;
using Aspire.Hosting.Azure.AppContainers;
using Aspire.Hosting.Lifecycle;
using Azure.Provisioning;
using Azure.Provisioning.AppContainers;
using Azure.Provisioning.Expressions;
using Azure.Provisioning.Resources;
using Google.Protobuf.WellKnownTypes;

namespace Aspire.Hosting;

internal class RegistryOverrideLifecycleHook(
DistributedApplicationExecutionContext executionContext) : IDistributedApplicationLifecycleHook
{
    public async Task BeforeStartAsync(DistributedApplicationModel appModel, CancellationToken cancellationToken = default)
    {
        if (executionContext.IsRunMode)
        {
            return;
        }

        // TODO: We need to support direct association between a compute resource and the container app environment.
        // Right now we support a single container app environment as the one we want to use and we'll fall back to 
        // azd based environment if we don't have one.

        var caes = appModel.Resources.OfType<AzureContainerAppEnvironmentResource>().ToArray();

        if (caes is not { Length: 1 })
        {
            throw new NotSupportedException("Can not use container app registry with out exactly one CAE.");
        }

        Func<AzureContainerAppEnvironmentResource, AzureContainerRegistryResource, Action<AzureResourceInfrastructure, ContainerApp>> getRegistryConfgiurationAction =
          static (AzureContainerAppEnvironmentResource environmentResource, AzureContainerRegistryResource registryResource) =>
            (AzureResourceInfrastructure infrastructure, ContainerApp containerApp) =>
            {

                BicepOutputReference registryEndpoint = new(AzureContainerConstants.AZURE_CONTAINER_REGISTRY_ENDPOINT, registryResource);
                BicepOutputReference identity = new(AzureContainerConstants.AZURE_CONTAINER_REGISTRY_MANAGED_IDENTITY_ID, environmentResource);

                string parameterName = $"{infrastructure.AspireResource.Name}_containerimage";
                containerApp.Template.Containers[0].Value.Image = new ProvisioningParameter(parameterName, typeof(string));
                infrastructure.AspireResource.Parameters[parameterName] = "{{{{ .Image }}}}";

                var identityParameter = identity.AsProvisioningParameter(infrastructure);
                var registryCredential = new ContainerAppRegistryCredentials
                {
                    Server = registryEndpoint.AsProvisioningParameter(infrastructure),
                    Identity = identityParameter,
                };
                containerApp.Configuration.Registries = [registryCredential];

                var id = BicepFunction.Interpolate($"{identityParameter}").Compile().ToString();

                containerApp.Identity.ManagedServiceIdentityType = ManagedServiceIdentityType.UserAssigned;
                containerApp.Identity.UserAssignedIdentities[id] = new UserAssignedIdentityDetails();
            };

        foreach (var r in appModel.Resources)
        {
            if (r.TryGetLastAnnotation<ManifestPublishingCallbackAnnotation>(out var lastAnnotation) && lastAnnotation == ManifestPublishingCallbackAnnotation.Ignore)
            {
                continue;
            }

            if (!r.IsContainer() && r is not ProjectResource)
            {
                continue;
            }
            var registry = appModel.Resources.OfType<AzureContainerRegistryResource>().FirstOrDefault();

            if (registry is null)
            {
                throw new NotSupportedException("Can not use container app registry with out a registry.");
            }

            r.Annotations.Add(new ContainerImageAnnotation { Image = string.Empty });
            r.Annotations.Add(new AzureContainerAppCustomizationAnnotation(getRegistryConfgiurationAction(caes[0], registry)));
        }

    }

}

