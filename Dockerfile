FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY src/FootballTickets.Domain/*.csproj src/FootballTickets.Domain/
COPY src/FootballTickets.Application/*.csproj src/FootballTickets.Application/
COPY src/FootballTickets.Infrastructure/*.csproj src/FootballTickets.Infrastructure/
COPY src/FootballTickets.Api/*.csproj src/FootballTickets.Api/
RUN dotnet restore src/FootballTickets.Api/FootballTickets.Api.csproj
COPY src/ src/
RUN dotnet publish src/FootballTickets.Api/FootballTickets.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "FootballTickets.Api.dll"]
