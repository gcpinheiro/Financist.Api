FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Financist.sln ./
COPY dotnet-tools.json ./
COPY src/Financist.Api/Financist.Api.csproj src/Financist.Api/
COPY src/Financist.Application/Financist.Application.csproj src/Financist.Application/
COPY src/Financist.Domain/Financist.Domain.csproj src/Financist.Domain/
COPY src/Financist.Infrastructure/Financist.Infrastructure.csproj src/Financist.Infrastructure/
COPY tests/Financist.UnitTests/Financist.UnitTests.csproj tests/Financist.UnitTests/
COPY tests/Financist.IntegrationTests/Financist.IntegrationTests.csproj tests/Financist.IntegrationTests/

RUN dotnet restore Financist.sln

COPY . .
RUN dotnet publish src/Financist.Api/Financist.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Financist.Api.dll"]
