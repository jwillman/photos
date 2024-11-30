param location string
param eventGridTopicName string
param functionAppEndpoint string
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
        endpointUrl: functionAppEndpoint
      }
    }
    filter: {
      includedEventTypes: [
        'Microsoft.Storage.BlobCreated'
      ]
    }
  }
}
