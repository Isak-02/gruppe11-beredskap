FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["gruppe11-beredskap/gruppe11-beredskap.csproj", "gruppe11-beredskap/"]
RUN dotnet restore "gruppe11-beredskap/gruppe11-beredskap.csproj"
COPY . .
WORKDIR "/src/gruppe11-beredskap"
RUN dotnet build "./gruppe11-beredskap.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./gruppe11-beredskap.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "gruppe11-beredskap.dll"]
