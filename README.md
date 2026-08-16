# Booking System

A modern, scalable restaurant booking system built with ASP.NET Core. The system enables users to discover restaurants, make reservations, manage bookings, and maintain favorite restaurants, while allowing restaurant managers to oversee tables and bookings.

## Features

### User Features
- **User Authentication & Management**: Secure JWT-based authentication with role-based access control (guest, manager, admin)
- **Restaurant Discovery**: Browse and search available restaurants
- **Bookings**: Make, confirm, and cancel restaurant reservations with time slot management
- **Favorite Restaurants**: Save and manage a personalized list of favorite dining establishments
- **Booking Management**: View booking history and manage active reservations

### Manager Features
- **Restaurant Management**: Manage restaurant details, tables, and availability
- **Booking Oversight**: View and manage all bookings for their restaurants
- **Table Management**: Configure tables, set capacities, and manage availability
- **Booking Confirmation**: Accept or reject guest reservations

### System Features
- **Background Jobs**: Hangfire-based job scheduling for recurring tasks
- **Structured Logging**: Serilog integration with Seq for centralized log aggregation
- **Caching**: Redis-backed caching for improved performance
- **Validation**: Fluent validation for all API inputs
- **OpenAPI Documentation**: Interactive API documentation with Scalar UI

## Tech Stack

- **Framework**: .NET 10.0, ASP.NET Core
- **Database**: PostgreSQL with Entity Framework Core and Dapper
- **Cache**: Redis
- **Background Jobs**: Hangfire
- **Logging**: Serilog with Seq sink
- **Validation**: FluentValidation
- **Authentication**: JWT Bearer tokens
- **Documentation**: OpenAPI/Swagger
- **Containerization**: Docker & Docker Compose
- **Development Environment**: Nix Flakes (optional)

## Prerequisites

- Docker & Docker Compose
- .NET 10.0 SDK (for local development)
- Redis (included via Docker Compose)
- PostgreSQL (included via Docker Compose)
- Seq (included via Docker Compose)

## Getting Started

### Quick Start with Docker Compose

1. **Clone the repository**
   ```bash
   git clone https://github.com/Monavixx/BookingSystem
   cd BookingSystem
   ```

2. **Start all at once**
   ```bash
   docker compose --profile app up
   ```

   This will start:
   - PostgreSQL database (`BookingSystemDB` on port 5432)
   - Redis cache (on port 6379)
   - Seq logging UI (on port 5341)
   This also will apply migrations to the database.

   When everything starts up, postgres healthcheck passed, migrations get applied, then the application runs.
   It will be available at `http://localhost:8080`

3. **Or just start the infrastructure**
   ```bash
   docker compose up
   ```
   This'll do everything that does the preceding command except for running the app. This can be used in development because you can just run `dotnet run --project BookingSystem.Api` or `./run.sh` that starts the application in the development environment.

### Local Development

1. **Access the API**
   - OpenAPI Documentation: `http://localhost:8080/openapi`
   - Scalar UI: `http://localhost:8080/scalar/v1`
   - Hangfire Dashboard: `http://localhost:8080/hangfire`

2. **Build and run the application as a container**

```bash
docker compose --profile app up --build
```

## Configuration

### Environment Variables

Configure the application via `appsettings.json` or `appsettings.[Development|Production|Test].json` in the `BookingSystem.Api` directory.

## Database Migrations

Entity Framework Core migrations are handled automatically on `docker compose up`. The database schema is initialized from migrations stored in `BookingSystem.Application/Migrations`.

## API Documentation

Once the application is running (Development environment only!), access the API documentation:

- **Scalar UI** (Interactive): `http://localhost:8080/scalar/v1`
- **OpenAPI JSON**: `http://localhost:8080/openapi/v1.json`
- **Hangfire Dashboard**: `http://localhost:8080/hangfire`
- **Seq Logs**: `http://localhost:5341`

## Testing

Run the test suite:

```bash
dotnet test
```

## Logging

View logs in real-time via Seq UI at `http://localhost:5341`

## Project Structure

- **BookingSystem.Api**: Web API layer with controllers and middleware
- **BookingSystem.Application**: Business logic, use cases, and application services
- **BookingSystem.Domain**: Domain entities, value objects, and domain events
- **BookingSystem.Infrastructure**: External service implementations and database access
- **BookingSystem.Tests**: Unit and integration tests

> For detailed architecture information, see `ARCHITECTURE.md`

## Default Credentials

**Seq Logging UI:**
- Username: `admin`
- Password: `admin`

**Redis:**
- Password: `admin`

**PostgreSQL:**
- Username: `postgres`
- Password: `admin`
- Database: `BookingSystemDB`

> ⚠️ Change these credentials in production environments!

## Development Workflow

### Code Quality
- Nullable reference types enabled
- Implicit usings enabled
- XML documentation generation for API
- EditorConfig compliance

## Contributing

Follow these guidelines when contributing:

1. Ensure all tests pass: `dotnet test`
2. Follow code style defined in `.editorconfig`
3. Write meaningful commit messages
4. Reference issues in commit messages where applicable

## Support

For issues, questions, or contributions, please open an issue in the repository.
