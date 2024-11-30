param location string
param functionAppName string
param appServicePlanId string
param storageAccountConnectionString string

resource functionApp 'Microsoft.Web/sites@2021-02-01' = {
  name: functionAppName
  location: location
  properties: {
    serverFarmId: appServicePlanId
    siteConfig: {
      appSettings: [
        {
          name: 'FUNCTIONS_EXTENSION_VERSION'
          value: '~4'
        }
        {
          name: 'FUNCTIONS_WORKER_RUNTIME'
          value: 'dotnet-isolated'
        }
        {
          name: 'AzureWebJobsStorage'
          value: storageAccountConnectionString
        }
      ]
    }
  }
}

output functionAppUrl string = 'https://${functionApp.name}.azurewebsites.net'
