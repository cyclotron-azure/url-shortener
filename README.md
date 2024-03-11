# Cyclotron Url Shortener

Cyclotron.com URL Shortener

This project is based on [AzUrlShortener](https://github.com/microsoft/AzUrlShortener).


## Pre-requisites

[Azure Developer CLI](https://learn.microsoft.com/en-us/azure/developer/azure-developer-cli/install-azd)


## Development

```bash
    #
    dotnet user-secrets init

 
   
    dotnet publish -c Release -o ./dist

    swa deploy src/UrlShortener.BlazorAdmin/dist/ -R rg-shorturl -n stapp-web --env prod --deployment-token 
```


## Notes

dotnet new blazorwasm -au SingleOrg --client-id "41d88d81-7392-4a43-8a12-639a25f83161" --tenant-id "common" -o BlazorSample


Accounts in this organizational directory only (Contoso only - Single tenant)

http://localhost:5049/auth-response
http://localhost:5049/authentication/login-callback

https://learn.microsoft.com/en-us/entra/identity-platform/quickstart-register-app#register-a-new-application-using-the-azure-portal