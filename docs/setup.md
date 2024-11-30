# Setup Instructions

Follow these steps to configure the project and enable automated deployments.

## 1. Create Azure Credentials

Log in to Azure CLI:

az login

## 2. Create a Service Principal:

az ad sp create-for-rbac --name "github-deployment" --role contributor --scopes /subscriptions/<SUBSCRIPTION_ID> --sdk-auth

Replace <SUBSCRIPTION_ID> with your Azure subscription ID:

az account show --query id -o tsv

## 3. Copy the JSON output (e.g.):

{
"clientId": "your-client-id",
"clientSecret": "your-client-secret",
"subscriptionId": "your-subscription-id",
"tenantId": "your-tenant-id",
...
}

## 4. Add Secrets in GitHub

-   Go to your repository on GitHub.
-   Navigate to Settings > Secrets and variables > Actions.
-   Add the following secrets:
    -   AZURE_CREDENTIALS: Paste the JSON output from the Service Principal creation.
    -   RESOURCE_GROUP: Enter the name of the resource group (e.g., Photos).
    -   FUNCTIONAPP_PUBLISH_PROFILE:
        Download the publish profile for your Function App from the Azure Portal.
        Copy the contents of the .publishsettings file.
        Add it as a secret named FUNCTIONAPP_PUBLISH_PROFILE.

## 5. Create Resource Group

(Optional, if not already created) Create the resource group in Azure:

az group create --name Photos --location "West Europe"
