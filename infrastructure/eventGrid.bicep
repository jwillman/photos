@description('The location for the Event Grid resources')
param location string

@description('The name of the Event Grid topic')
param eventGridTopicName string

@description('The URL of the Azure Function App endpoint')
param functionAppUrl string

@description('The storage account resource ID')
param storageAccountId string

resource systemTopic 'Microsoft.EventGrid/systemTopics@2021-06-01-preview' = {
  name: eventGridTopicName
  location: location
  properties: {
    source: storageAccountId
    topicType: 'Microsoft.Storage.StorageAccounts'
  }
}

resource eventSubscription 'Microsoft.EventGrid/systemTopics/eventSubscriptions@2021-06-01-preview' = {
  name: 'functionTriggerSubscription'
  parent: systemTopic
  properties: {
    destination: {
      endpointType: 'WebHook'
      properties: {
        endpointUrl: functionAppUrl  // URL of the Azure Function App
      }
    }
    filter: {
      includedEventTypes: [
        'Microsoft.Storage.BlobCreated'  // Trigger only for BlobCreated events
      ]
    }
  }
}
