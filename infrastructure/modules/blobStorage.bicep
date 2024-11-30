@description('The location for the storage account')
param location string

@description('The name of the storage account')
param storageAccountName string

@description('The name of the photo container')
param photoContainerName string = 'photos'

@description('The name of the metadata container')
param metadataContainerName string = 'metadata'

resource storageAccount 'Microsoft.Storage/storageAccounts@2021-09-01' = {
  name: storageAccountName
  location: location
  kind: 'StorageV2'
  sku: {
    name: 'Standard_LRS'
  }
  properties: {
    allowBlobPublicAccess: true  // Allow public access for blobs
  }
}

resource photoContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2021-09-01' = {
  name: '${storageAccount.name}/default/${photoContainerName}'
  properties: {
    publicAccess: 'Blob'  // Public access for blob data
  }
}

resource metadataContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2021-09-01' = {
  name: '${storageAccount.name}/default/${metadataContainerName}'
  properties: {
    publicAccess: 'Blob'  // Public access for blob data
  }
}

output storageAccountId string = storageAccount.id
output storageConnectionString string = listKeys(storageAccount.id, '2021-09-01').keys[0].value
