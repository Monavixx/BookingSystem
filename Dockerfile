FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["BookingSystem.Domain/BookingSystem.Domain.csproj", "BookingSystem.Domain/"]
COPY ["BookingSystem.Application/BookingSystem.Application.csproj", "BookingSystem.Application/"]
COPY ["BookingSystem.Infrastructure/BookingSystem.Infrastructure.csproj", "BookingSystem.Infrastructure/"]
COPY ["BookingSystem.Api/BookingSystem.Api.csproj", "BookingSystem.Api/"]

RUN dotnet restore "BookingSystem.Api/BookingSystem.Api.csproj"

#Add .dockerignore
COPY . .

WORKDIR /src/BookingSystem.Api
RUN dotnet build "BookingSystem.Api.csproj" -c Release -o /app/build --no-restore

FROM build AS publish
RUN dotnet publish "BookingSystem.Api.csproj" -c Release -o /app/publish --no-restore /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS production
WORKDIR /app
EXPOSE 8080
RUN apt-get update && apt-get install -y libgssapi-krb5-2 && rm -rf /var/lib/apt/lists/*

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "BookingSystem.Api.dll"]
