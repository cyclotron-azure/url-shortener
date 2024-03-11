metadata description = 'Creates an Azure Static Web Apps instance.'
param name string
param location string = resourceGroup().location
param tags object = {}

param githubUrl string

param sku object = {
    name: 'Standard'
    tier: 'Standard'
}

resource web 'Microsoft.Web/staticSites@2022-03-01' = {
  name: name
  location: location
  tags: tags
  sku: sku
  properties: {
    repositoryUrl: githubUrl
    branch: 'main'
    buildProperties: {
      appLocation: './src/UrlShortener.BlazorAdmin'
      outputLocation: 'wwwroot'
    }
    stagingEnvironmentPolicy: 'Enabled'
    allowConfigFileUpdates: true
    provider: 'GitHub'
    enterpriseGradeCdnStatus: 'Disabled'
  }
}

output name string = web.name
output uri string = 'https://${web.properties.defaultHostname}'
