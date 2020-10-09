#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/runtime:3.1-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["AnimusHeart2Influx/AnimusHeart2Influx.csproj", "AnimusHeart2Influx/"]
RUN dotnet restore "AnimusHeart2Influx/AnimusHeart2Influx.csproj"
COPY . .
WORKDIR "/src/AnimusHeart2Influx"
RUN dotnet build "AnimusHeart2Influx.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AnimusHeart2Influx.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AnimusHeart2Influx.dll"]