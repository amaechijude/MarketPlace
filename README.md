# MarketPlace

A modern, scalable marketplace platform built with **.NET 10**, **ASP.NET Core**, and **Microsoft Aspire**. This project demonstrates enterprise-grade patterns and best practices for building distributed, cloud-ready marketplace applications.

## 📋 Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Project Structure](#project-structure)
- [Tech Stack](#tech-stack)
- [Prerequisites](#prerequisites)
- [Getting Started](#getting-started)
- [Configuration](#configuration)
- [Running the Application](#running-the-application)
- [API Documentation](#api-documentation)
- [Database](#database)
- [Caching & Performance](#caching--performance)
- [Email & Notifications](#email--notifications)
- [File Storage](#file-storage)
- [Testing](#testing)

## 🎯 Overview

MarketPlace is a comprehensive e-commerce platform designed to handle complex marketplace operations. It leverages modern cloud-native technologies, containerization with Microsoft Aspire, and implements industry-standard patterns for scalability, resilience, and maintainability.

## ✨ Features

- **Scalable API Architecture** - RESTful API built with ASP.NET Core Web API
- **Cloud-Ready** - Microsoft Aspire for container orchestration and service configuration
- **Database Support** - PostgreSQL integration with Entity Framework Core
- **Caching Layer** - Redis for distributed caching and rate limiting
- **Email Services** - FluentEmail with multiple renderers (Liquid, Razor, SMTP)
- **Image Processing** - SixLabors.ImageSharp and SkiaSharp for image handling
- **AWS Integration** - S3 support for file storage
- **Validation** - FluentValidation for robust input validation
- **Authentication** - Google OAuth integration
- **Resilience** - Polly for resilient HTTP calls with retry policies
- **Rate Limiting** - Redis-based rate limiting
- **Comprehensive Testing** - Unit and integration test suite with TestContainers
- **OpenAPI/Swagger** - Built-in API documentation

## 📁 Project Structure

```
MarketPlace/
├── MarketPlace.Api/              # Main API project
│   ├── Controllers/              # API endpoints
│   ├── Models/                   # Data models and DTOs
│   ├── Services/                 # Business logic
│   ├── Validators/               # FluentValidation rules
│   └── ...
├── MarketPlace.AppHost/          # Aspire AppHost orchestration
│   ├── Program.cs               # Service configuration and deployment
│   └── ...
├── MarketPlace.ServiceDefaults/  # Shared service configurations
│   ├── Extensions/              # Extension methods
│   └── ...
├── MarketPlace.Test/             # Test project
│   ├── Unit/                    # Unit tests
│   ├── Integration/             # Integration tests
│   └── ...
├── MarketPlace.slnx             # Solution file
├── Directory.Build.props         # Build configuration
├── Directory.Packages.props      # Centralized package management
├── aspire.config.json           # Aspire configuration
├── .env                         # Environment variables
└── README.md                    # This file
```

## 🛠️ Tech Stack

| Layer | Technology | Purpose |
|-------|-----------|---------|
| **Framework** | .NET 10, ASP.NET Core | Web API framework |
| **Orchestration** | Microsoft Aspire | Service orchestration & configuration |
| **Database** | PostgreSQL | Primary data store |
| **ORM** | Entity Framework Core | Data access layer |
| **Caching** | Redis | Distributed caching |
| **Email** | FluentEmail | Email service abstraction |
| **Validation** | FluentValidation | Input validation |
| **Storage** | AWS S3 | File storage |
| **Imaging** | SixLabors.ImageSharp, SkiaSharp | Image processing |
| **Auth** | Google OAuth | Authentication provider |
| **HTTP Resilience** | Polly | Retry policies & circuit breakers |
| **API Documentation** | Scalar.AspNetCore | Interactive API documentation |
| **Testing** | TestContainers | Docker-based integration testing |

## 📦 Prerequisites

- **.NET 10 SDK** - [Download](https://dotnet.microsoft.com/download/dotnet)
- **Docker & Docker Compose** - For Aspire and TestContainers
- **AWS Account** (optional) - For S3 integration
- **Google OAuth Credentials** (optional) - For authentication

## 🚀 Getting Started

### 1. Clone the Repository

```bash
git clone https://github.com/amaechijude/MarketPlace.git
cd MarketPlace
```

### 2. Install Dependencies

```bash
dotnet restore
```

### 3. Set Up Environment Variables

Create a `.env` file in the root directory:

```env
# Email Configuration
SMTP_HOST=smtp.gmail.com
SMTP_PORT=587
SMTP_USERNAME=your-email@gmail.com
SMTP_PASSWORD=your-app-password

# AWS S3
AWS_ACCESS_KEY_ID=your_access_key
AWS_SECRET_ACCESS_KEY=your_secret_key
AWS_S3_BUCKET=your-bucket-name
AWS_REGION=us-east-1

# Google OAuth
GOOGLE_CLIENT_ID=your_google_client_id
GOOGLE_CLIENT_SECRET=your_google_client_secret

# Application
ASPNETCORE_ENVIRONMENT=Development
API_PORT=5000
```

## ⚙️ Configuration

The application is configured through Microsoft Aspire. Database and Redis services are automatically provisioned and managed by the Aspire orchestrator.

## 🏃 Running the Application

### Using Aspire (Recommended)

```bash
aspire run
```

This will automatically start all services (API, Redis, PostgreSQL) and provide a dashboard at `http://localhost:18888`.

The API will be available at `https://localhost:7000`.

## 📚 API Documentation

Once running, access the interactive API documentation:

- **Scalar UI**: `https://localhost:7000/scalar/v1`
- **OpenAPI JSON**: `https://localhost:7000/openapi/v1.json`

## 💾 Database

### Entity Framework Core

This project uses Entity Framework Core with PostgreSQL. The database and migrations are managed through the Aspire orchestrator.

### Creating a New Migration

```bash
cd MarketPlace.Api
dotnet ef migrations add MigrationName
dotnet ef database update
```

## ⚡ Caching & Performance

### Redis Integration

- **Distributed Caching**: `IDistributedCache` for multi-instance deployments
- **Rate Limiting**: Redis-backed rate limiting via `RedisRateLimiting.AspNetCore`
- **Hybrid Caching**: L1 (in-memory) + L2 (Redis) for optimal performance

Redis is automatically provisioned by the Aspire orchestrator.

## 📧 Email & Notifications

### FluentEmail

Multiple rendering engines for templates:

- **Liquid**: Dynamic templating
- **Razor**: Type-safe templates
- **SMTP**: Direct email sending

```csharp
// Send email
await emailService.SendAsync(recipient, subject, body);
```

## 📁 File Storage

### AWS S3

```csharp
// Upload file to S3
var url = await storageService.UploadAsync(file, bucket, key);
```

### Image Processing

- **SixLabors.ImageSharp**: Modern image manipulation
- **SkiaSharp**: Vector graphics and advanced processing

## ✅ Testing

### Run All Tests

```bash
dotnet test
```

### Run Specific Test Project

```bash
dotnet test MarketPlace.Test/MarketPlace.Test.csproj
```

### Test Environment

Tests use **TestContainers** for PostgreSQL, ensuring isolated and consistent test environments without requiring manual database setup.

```bash
# Tests automatically provision Docker containers
dotnet test --verbosity minimal
```

### Test Coverage

```bash
# Using OpenCover
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

## 🔐 Security Considerations

- Use strong environment variable secrets in production
- Enable HTTPS only
- Implement proper authentication/authorization
- Validate all user inputs (FluentValidation)
- Use rate limiting to prevent abuse
- Implement CORS policy appropriately
- Regularly update dependencies

---

**Built with ❤️ using .NET and Microsoft Aspire**
