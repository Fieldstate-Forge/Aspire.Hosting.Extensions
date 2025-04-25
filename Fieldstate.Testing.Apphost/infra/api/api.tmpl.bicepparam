using './api.module.bicep'

param api_containerimage = '{{ .Image }}'
param api_containerport = '{{ targetPortOrDefault 8080 }}'
param cae_outputs_azure_container_apps_environment_default_domain = '{{ .Env.CAE_AZURE_CONTAINER_APPS_ENVIRONMENT_DEFAULT_DOMAIN }}'
param cae_outputs_azure_container_apps_environment_id = '{{ .Env.CAE_AZURE_CONTAINER_APPS_ENVIRONMENT_ID }}'
param cae_outputs_azure_container_registry_managed_identity_id = '{{ .Env.CAE_AZURE_CONTAINER_REGISTRY_MANAGED_IDENTITY_ID }}'
param registry_outputs_azure_container_registry_endpoint = '{{ .Env.REGISTRY_AZURE_CONTAINER_REGISTRY_ENDPOINT }}'
