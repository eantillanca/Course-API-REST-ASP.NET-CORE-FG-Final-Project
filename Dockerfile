FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["MoviesAPI.csproj", "./"]
RUN dotnet restore "MoviesAPI.csproj"
COPY . .
WORKDIR "/src/"
RUN dotnet build "MoviesAPI.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Para usar dotnet watch durante el desarrollo
FROM build AS dev
WORKDIR /src
CMD ["dotnet", "watch", "run"]

# Para la publicación final
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "MoviesAPI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MoviesAPI.dll"]
