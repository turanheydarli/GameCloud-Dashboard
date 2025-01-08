FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["GC.Dashboard/GC.Dashboard.csproj", "GC.Dashboard/"]
RUN dotnet restore "GC.Dashboard/GC.Dashboard.csproj"
COPY . .
WORKDIR "/src/GC.Dashboard"
RUN dotnet build "GC.Dashboard.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "GC.Dashboard.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GC.Dashboard.dll"]
