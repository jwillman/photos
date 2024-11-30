param location string
param storageAccountName string
param photoContainerName string
param metadataContainerName string

resource storageAccount 'Microsoft.Storage/storageAccounts@2021-09-01' = {
  name: storageAccountName
  location: location
  kind: 'StorageV2'
  sku: {
    name: 'Standard_LRS'
  }
}

resource photoContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2021-09-01' = {
  name: '${storageAccount.name}/default/${photoContainerName}'
  properties: {
    publicAccess: 'Blob'
  }
}

resource metadataContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2021-09-01' = {
  name: '${storageAccount.name}/default/${metadataContainerName}'
  properties: {
    publicAccess: 'Blob'
  }
}

output storageAccountId string = storageAccount.id
output storageConnectionString string = listKeys(storageAccount.id, '2021-09-01').keys[0].value
