# Stage 1: Base runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8081

# Stage 2: Build environment
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["GameCloud.Dashboard/GameCloud.Dashboard.csproj", "GameCloud.Dashboard/"]
RUN dotnet restore "GameCloud.Dashboard/GameCloud.Dashboard.csproj"
COPY . .

WORKDIR "/src/GameCloud.Dashboard"
RUN dotnet build "GameCloud.Dashboard.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Stage 3: Publish artifacts
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "GameCloud.Dashboard.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Stage 4: Final runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GameCloud.Dashboard.dll"]
