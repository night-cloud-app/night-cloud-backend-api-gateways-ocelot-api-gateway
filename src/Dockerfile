﻿FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["api-gateways/ocelot-api-gateway/src/OcelotApiGateway.csproj", "api-gateways/ocelot-api-gateway/src/"]
RUN dotnet restore "api-gateways/ocelot-api-gateway/src/OcelotApiGateway.csproj"
COPY . .
WORKDIR "/src/api-gateways/ocelot-api-gateway/src"
RUN dotnet build "OcelotApiGateway.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OcelotApiGateway.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OcelotApiGateway.dll"]
