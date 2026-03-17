FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["StealAllTheCats/StealAllTheCats.csproj", "StealAllTheCats/"]
COPY ["StealAllTheCats.Common/StealAllTheCats.Common.csproj", "StealAllTheCats.Common/"]
COPY ["StealAllTheCats.Database/StealAllTheCats.Database.csproj", "StealAllTheCats.Database/"]
RUN dotnet restore "StealAllTheCats/StealAllTheCats.csproj"
COPY . .
WORKDIR "/src/StealAllTheCats"
RUN dotnet build "StealAllTheCats.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "StealAllTheCats.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "StealAllTheCats.dll"]
