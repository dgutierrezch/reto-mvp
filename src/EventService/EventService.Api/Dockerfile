FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY src/Shared/EventPlatform.Contracts/EventPlatform.Contracts.csproj Shared/EventPlatform.Contracts/
COPY src/EventService/EventService.Domain/EventService.Domain.csproj EventService/EventService.Domain/
COPY src/EventService/EventService.Application/EventService.Application.csproj EventService/EventService.Application/
COPY src/EventService/EventService.Infrastructure/EventService.Infrastructure.csproj EventService/EventService.Infrastructure/
COPY src/EventService/EventService.Api/EventService.Api.csproj EventService/EventService.Api/

RUN dotnet restore EventService/EventService.Api/EventService.Api.csproj

COPY src/Shared/ Shared/
COPY src/EventService/ EventService/

RUN dotnet publish EventService/EventService.Api/EventService.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "EventService.Api.dll"]
