@description('The location for the resource(s) to be deployed.')
param location string = resourceGroup().location

param registryPrinicpalId string

resource registry 'Microsoft.ContainerRegistry/registries@2023-07-01' = {
  name: take('registry${uniqueString(resourceGroup().id)}', 50)
  location: location
  sku: {
    name: 'Basic'
  }
}

resource registry_AcrPull 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(registry.id, registryPrinicpalId, subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '7f951dda-4ed3-4680-a7ca-43fe172d538d'))
  properties: {
    principalId: registryPrinicpalId
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '7f951dda-4ed3-4680-a7ca-43fe172d538d')
    principalType: 'ServicePrincipal'
  }
  scope: registry
}

output AZURE_CONTAINER_REGISTRY_NAME string = registry.name

output AZURE_CONTAINER_REGISTRY_ENDPOINT string = registry.properties.loginServer

output AZURE_CONTAINER_REGISTRY_ID string = registry.id