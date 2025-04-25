using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Azure.AppContainers;
using Aspire.Hosting.Azure;
using Azure.Provisioning.Authorization;
using Azure.Provisioning.ContainerRegistry;
using Azure.Provisioning.Expressions;
using Azure.Provisioning;

namespace Aspire.Hosting;

public static class AzureContainerRegistryResourceBuilderExtensions
{
    private const string registryPrinicpalId = nameof(registryPrinicpalId);
    public static IResourceBuilder<AzureContainerAppEnvironmentResource> WithReference(this IResourceBuilder<AzureContainerAppEnvironmentResource> builder, IResourceBuilder<AzureContainerRegistryResource> registryBuilder)
    {
        var managedIdentity = builder.GetOutput(AzureContainerConstants.MANAGED_IDENTITY_PRINCIPAL_ID);
        var roleDefinitionId = BicepFunction.GetSubscriptionResourceId("Microsoft.Authorization/roleDefinitions", ContainerRegistryBuiltInRole.AcrPull.ToString());

        registryBuilder.ConfigureInfrastructure(infrastructure =>
        {
            if (infrastructure.GetProvisionableResources().OfType<ContainerRegistryService>().FirstOrDefault() is { } containerRegistry)
            {
                var principalId = new ProvisioningParameter(registryPrinicpalId, typeof(Guid));
                infrastructure.Add(principalId);

                var pullRa = containerRegistry.CreateRoleAssignment(ContainerRegistryBuiltInRole.AcrPull,
                    RoleManagementPrincipalType.ServicePrincipal, principalId);

                pullRa.Name = BicepFunction.CreateGuid(containerRegistry.Id, principalId, pullRa.RoleDefinitionId);
                infrastructure.Add(pullRa);
            }

        }).WithParameter(registryPrinicpalId, managedIdentity);

        builder.ConfigureInfrastructure(infrastructure =>
        {
            var provisionbleResources = infrastructure.GetProvisionableResources();
            if (provisionbleResources.OfType<ContainerRegistryService>().FirstOrDefault() is { } containerRegistry)
            {
                if (provisionbleResources.OfType<RoleAssignment>().FirstOrDefault(_ =>
                {
                    return _.RoleDefinitionId.Value! == roleDefinitionId.Value!;
                }) is { } roleAssignment)
                {
                    infrastructure.Remove(roleAssignment);
                }
                infrastructure.Remove(containerRegistry);
            }

            var outputParameters = infrastructure.GetProvisionableResources().OfType<ProvisioningOutput>();
            if (outputParameters.FirstOrDefault(_ => _.BicepIdentifier == AzureContainerConstants.AZURE_CONTAINER_REGISTRY_NAME) is { } registryName)
            {
                infrastructure.Remove(registryName);
            }
            if (outputParameters.FirstOrDefault(_ => _.BicepIdentifier == AzureContainerConstants.AZURE_CONTAINER_REGISTRY_ENDPOINT) is { } registryEndpoint)
            {
                infrastructure.Remove(registryEndpoint);
            }
        });

        return builder;
    }
}