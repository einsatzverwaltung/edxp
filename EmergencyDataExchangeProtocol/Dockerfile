#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:2.2-stretch-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:2.2-stretch AS build
WORKDIR /src
COPY ["EmergencyDataExchangeProtocol/EmergencyDataExchangeProtocol.csproj", "EmergencyDataExchangeProtocol/"]
RUN dotnet restore "EmergencyDataExchangeProtocol/EmergencyDataExchangeProtocol.csproj"
COPY . .
WORKDIR "/src/EmergencyDataExchangeProtocol"
RUN dotnet build "EmergencyDataExchangeProtocol.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EmergencyDataExchangeProtocol.csproj" -c Release -o /app/publish

FROM base AS final

ENV DB_SERVER "localhost"
ENV DB_PORT "27017"
ENV DB_NAME "admin"
ENV DB_USER ""
ENV DB_PASSWORD ""

WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EmergencyDataExchangeProtocol.dll"]