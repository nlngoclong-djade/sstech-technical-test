FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Partner_Integration_BFF/Partner_Integration_BFF.csproj", "Partner_Integration_BFF/"]
RUN dotnet restore "Partner_Integration_BFF/Partner_Integration_BFF.csproj"
COPY . .
WORKDIR "/src/Partner_Integration_BFF"
RUN dotnet build "./Partner_Integration_BFF.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Partner_Integration_BFF.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Partner_Integration_BFF.dll"]
