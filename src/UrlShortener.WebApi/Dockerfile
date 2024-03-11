#
# build: docker build -t {id}.azurecr.io/url-shortener-webapi -f src/UrlShortener.WebApi/Dockerfile .
#
#
# push: docker push {id}.azurecr.io/url-shortener-webapi
#

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

USER app
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG configuration=Release
WORKDIR /src

COPY ["code-analysis.props", "code-analysis.props"]
COPY ["Directory.Build.props", "Directory.Build.props"]
COPY ["Directory.Packages.props", "Directory.Packages.props"]

COPY ["src/UrlShortener.Core/UrlShortener.Core.csproj", "src/UrlShortener.Core/"]
COPY ["src/UrlShortener.WebApi/UrlShortener.WebApi.csproj", "src/UrlShortener.WebApi/"]

RUN dotnet restore "src/UrlShortener.WebApi/UrlShortener.WebApi.csproj"
COPY . .
WORKDIR "/src/src/UrlShortener.WebApi"
RUN dotnet build "UrlShortener.WebApi.csproj" -c $configuration -o /app/build

FROM build AS publish
ARG configuration=Release
RUN dotnet publish "UrlShortener.WebApi.csproj" -c $configuration -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "UrlShortener.WebApi.dll"]
