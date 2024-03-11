param githubUrl string
param name string
param location string = resourceGroup().location
param tags object = {}

param serviceName string = 'web'

module web './staticwebapp.bicep' = {
  name: '${serviceName}-staticwebapp-module'
  params: {
    githubUrl: githubUrl
    name: name
    location: location
    tags: union(tags, { 'azd-service-name': serviceName })
  }
}

output SERVICE_WEB_NAME string = web.outputs.name
output SERVICE_WEB_URI string = web.outputs.uri
