@description('The location for all resources')
param location string = resourceGroup().location

@description('The name of the storage account')
param storageAccountName string

@description('The name of the function app')
param functionAppName string

@description('The name of the app service plan')
param appServicePlanName string

@description('The Event Grid system topic name')
param eventGridTopicName string

@description('The storage container name for photos')
param photoContainerName string = 'photos'

@description('The storage container name for metadata')
param metadataContainerName string = 'metadata'

module blobStorage './modules/blobStorage.bicep' = {
  name: 'blobStorageDeployment'
  params: {
    location: location
    storageAccountName: storageAccountName
    photoContainerName: photoContainerName
    metadataContainerName: metadataContainerName
  }
}

module appServicePlan './modules/appServicePlan.bicep' = {
  name: 'appServicePlanDeployment'
  params: {
    location: location
    appServicePlanName: appServicePlanName
  }
}

module functionApp './modules/functionApp.bicep' = {
  name: 'functionAppDeployment'
  params: {
    location: location
    functionAppName: functionAppName
    appServicePlanId: appServicePlan.outputs.id
    storageAccountConnectionString: blobStorage.outputs.storageConnectionString
  }
}

module eventGrid './modules/eventGrid.bicep' = {
  name: 'eventGridDeployment'
  params: {
    location: location
    eventGridTopicName: eventGridTopicName
    functionAppEndpoint: functionApp.outputs.functionAppUrl
    storageAccountId: blobStorage.outputs.storageAccountId
  }
}
