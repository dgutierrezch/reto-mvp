FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY src/Shared/EventPlatform.Contracts/EventPlatform.Contracts.csproj Shared/EventPlatform.Contracts/
COPY src/NotificationService/NotificationService.Domain/NotificationService.Domain.csproj NotificationService/NotificationService.Domain/
COPY src/NotificationService/NotificationService.Application/NotificationService.Application.csproj NotificationService/NotificationService.Application/
COPY src/NotificationService/NotificationService.Infrastructure/NotificationService.Infrastructure.csproj NotificationService/NotificationService.Infrastructure/
COPY src/NotificationService/NotificationService.Api/NotificationService.Api.csproj NotificationService/NotificationService.Api/

RUN dotnet restore NotificationService/NotificationService.Api/NotificationService.Api.csproj

COPY src/Shared/ Shared/
COPY src/NotificationService/ NotificationService/

RUN dotnet publish NotificationService/NotificationService.Api/NotificationService.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "NotificationService.Api.dll"]
