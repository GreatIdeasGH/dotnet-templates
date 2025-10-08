# Fundraiser

A modern, full-stack fundraising platform built with .NET 9.0 and React. This application enables organizations to create, manage, and track fundraising campaigns with image upload capabilities, real-time donations tracking, and comprehensive admin tools.

## 🌟 Features

- **Campaign Management**: Create, edit, and manage fundraising campaigns with rich content
- **Image Upload**: Support for campaign images with drag-and-drop functionality
- **Real-time Donations**: Track donations and campaign progress in real-time
- **Admin Dashboard**: Comprehensive admin interface for managing campaigns and donations
- **Responsive Design**: Mobile-first design with modern UI components
- **Authentication**: Secure JWT-based authentication system
- **Background Processing**: Asynchronous task processing with MassTransit

## 🏗 Architecture

This project follows Clean Architecture principles with clear separation of concerns:

### Backend (.NET 9.0)
- **Domain Layer**: Core business entities and domain logic
- **Application Layer**: Use cases, business rules, and application services
- **Infrastructure Layer**: Data access, external services, and persistence
- **WebAPI Layer**: REST API endpoints and HTTP concerns

### Frontend (React/TypeScript)
- **React 19**: Modern React with React Compiler optimization
- **TypeScript 5.8**: Type-safe development
- **TanStack Router**: Type-safe routing
- **TanStack Query**: Server state management
- **HeroUI**: Modern component library
- **TailwindCSS**: Utility-first CSS framework

### Infrastructure
- **PostgreSQL**: Primary database
- **Entity Framework Core**: ORM and data access
- **MassTransit**: Message bus for background processing
- **.NET Aspire**: Orchestration and service management

## 🚀 Quick Start

### Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Bun](https://bun.sh/) (JavaScript runtime and package manager)
- [PostgreSQL](https://www.postgresql.org/) (for database)

### Installation

1. **Clone the repository:**
   ```bash
   git clone https://github.com/GreatIdeasGH/Fundraiser.git
   cd Fundraiser
   ```

2. **Install .NET dependencies:**
   ```bash
   dotnet restore
   ```

3. **Install frontend dependencies:**
   ```bash
   cd src/greatideas.fundraiser.client
   bun install
   cd ../..
   ```

### Development Setup

#### Using Make Commands (Recommended)

```bash
# Build the solution
make build

# Run the API server
make run

# Run with Aspire orchestration
make aspires

# Run frontend development server
cd src/greatideas.fundraiser.client
bun run dev
```

#### Manual Commands

**Backend Development:**
```bash
# Build the solution
dotnet build --configuration Release --tl

# Run tests
dotnet test --configuration Release --tl

# Run the API server
dotnet run --project src/GreatIdeas.WebAPI
```

**Frontend Development:**
```bash
cd src/greatideas.fundraiser.client

# Start development server
bun run dev

# Build for production
bun run build

# Preview production build
bun run preview
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
├── aspire/                              # .NET Aspire orchestration
│   ├── GreatIdeas.AppHost/     # Aspire app host
│   └── GreatIdeas.ServiceDefaults/ # Shared service configuration
├── src/                                 # Source code
│   ├── GreatIdeas.Domain/       # Domain models and entities
│   ├── GreatIdeas.Application/  # Business logic and use cases
│   ├── GreatIdeas.Infrastructure/ # Data access and external services
│   ├── GreatIdeas.WebAPI/       # REST API controllers and endpoints
│   └── greatideas.fundraiser.client/       # React frontend application
├── tests/                               # Test projects
│   └── GreatIdeas.Test.WebAPI/  # API integration tests
├── Makefile                             # Development commands
├── Fundraiser.sln                       # Visual Studio solution
└── global.json                          # .NET SDK version configuration
```

## 🔧 Configuration

### Environment Variables

Create `.env` files in the appropriate directories:

**Backend (API root):**
```env
ConnectionStrings__DefaultConnection=Host=localhost;Database=Fundraiser;Username=your_user;Password=your_password
```

**Frontend (`src/greatideas.fundraiser.client/.env`):**
```env
VITE_WEBAPI_URL=https://localhost:5215/api
```

### Development Ports

- **Frontend**: http://localhost:5173
- **Backend API**: https://localhost:5215
- **Aspire Dashboard**: https://localhost:15020 (when using Aspire)

## 🧪 Testing

```bash
# Run all tests
dotnet test --configuration Release --tl

# Run specific test project
dotnet test tests/GreatIdeas.Test.WebAPI --configuration Release --tl

# Run tests with coverage
dotnet test --configuration Release --collect:"XPlat Code Coverage"
```

## 🎯 Key Features Implementation

### Campaign Management
- Create and edit fundraising campaigns
- Upload campaign images with validation
- Set fundraising goals and track progress
- Campaign status management (active, completed, draft)

### Image Upload System
- Drag-and-drop file upload interface
- Support for multiple image formats (JPEG, PNG, WebP)
- Automatic image optimization and conversion
- Secure file storage in `wwwroot/tempfiles`

### Admin Interface
- Campaign management dashboard
- Donation tracking and reporting
- User management capabilities
- Real-time campaign statistics

## 🚀 Deployment

### Using Docker

```bash
# Build Docker image
docker build -f src/GreatIdeas.WebAPI/Dockerfile -t fundraiser .

# Run with docker-compose
docker-compose up -d
```

### Production Build

```bash
# Build backend for production
dotnet build --configuration Release

# Build frontend for production
cd src/greatideas.fundraiser.client
bun run build
```

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

### Development Guidelines

- Follow Clean Architecture principles
- Write unit tests for new features
- Use TypeScript for all frontend code
- Follow existing code style and conventions
- Update documentation for significant changes

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🔗 Related Documentation

- [Campaign Enhancement Documentation](CAMPAIGN_ENHANCEMENT.md)
- [Image Upload Implementation](IMAGE_UPLOAD_IMPLEMENTATION.md)
- [Frontend Setup Guide](src/greatideas.fundraiser.client/README.md)

## 💬 Support

For support and questions, please open an issue in the GitHub repository.