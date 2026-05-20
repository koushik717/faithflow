FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore as distinct layers
COPY ["backend/src/FaithFlow.API/FaithFlow.API.csproj", "backend/src/FaithFlow.API/"]
COPY ["backend/src/FaithFlow.Application/FaithFlow.Application.csproj", "backend/src/FaithFlow.Application/"]
COPY ["backend/src/FaithFlow.Domain/FaithFlow.Domain.csproj", "backend/src/FaithFlow.Domain/"]
COPY ["backend/src/FaithFlow.Infrastructure/FaithFlow.Infrastructure.csproj", "backend/src/FaithFlow.Infrastructure/"]

RUN dotnet restore "backend/src/FaithFlow.API/FaithFlow.API.csproj"

# Copy everything else and build
COPY backend/src/ ./backend/src/
WORKDIR "/src/backend/src/FaithFlow.API"
RUN dotnet build "FaithFlow.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FaithFlow.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "FaithFlow.API.dll"]
