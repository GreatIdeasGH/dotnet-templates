# Company.Project

A modern backend built with .NET 9.0, ASP.NET Core, Entity Framework Core, MassTransit, PostgreSQL, and others. This project serves as a boilerplate for building scalable web APIs with best practices in mind.

## 🏗 Architecture

This project follows Clean Architecture principles with clear separation of concerns:

### Backend

- **Domain Layer**: Core business entities and domain logic
- **Application Layer**: Use cases, business rules, and application services
- **Infrastructure Layer**: Data access, external services, and persistence
- **WebAPI Layer**: REST API endpoints and HTTP concerns

### Infrastructure

- **PostgreSQL**: Primary database
- **Entity Framework Core**: ORM and data access
- **MassTransit**: Message bus for background processing
- **.NET Aspire**: Orchestration and service management

## 🚀 Quick Start

### Prerequisites

- [.NET SDK](https://dotnet.microsoft.com/download/dotnet)
- [PostgreSQL](https://www.postgresql.org/) (for database)

**Backend Development:**

```bash
# Build the solution
dotnet build Company.Project --tl

# Run tests
dotnet test Company.Project --tl

# Run the API server
dotnet run --project src/Company.Project.WebAPI
```

### Database Setup

```bash
# Apply database migrations
make ef-update

# Add new migration
make ef-add name=YourMigrationName

# Remove last migration
make ef-remove
```

## 📁 Project Structure

```
.
├── aspire/                               # .NET Aspire orchestration
│   ├── Company.Project.AppHost/          # Aspire app host
│   └── Company.Project.ServiceDefaults/  # Shared service default configuration
├── src/                                  # Source code
│   ├── Company.Project.Domain/           # Domain models and entities
│   ├── Company.Project.Application/      # Business logic and use cases
│   ├── Company.Project.Infrastructure/   # Data access and external services
│   ├── Company.Project.WebAPI/           # REST API services and endpoints
├── Makefile                              # Development commands
├── Company.Project.slnx                  # Visual Studio solution
└── global.json                           # .NET SDK version configuration
```

## 💬 Support

For support and questions, please open an issue in the GitHub repository.
