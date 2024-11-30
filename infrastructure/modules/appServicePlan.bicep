param location string
param appServicePlanName string

resource appServicePlan 'Microsoft.Web/serverfarms@2021-02-01' = {
  name: appServicePlanName
  location: location
  sku: {
    name: 'Y1'  // Consumption plan
    tier: 'Dynamic'
  }
  kind: 'functionapp' 
}

output id string = appServicePlan.id
