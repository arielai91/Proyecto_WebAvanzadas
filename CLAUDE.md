# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

PetFoundation is a pet adoption platform built with a microservices architecture: .NET 8.0/10.0 backend services, Angular 17 frontend, PostgreSQL, RabbitMQ, and Supabase for image storage.

## Common Commands

### Quick Start
```bash
cp .env.example .env
make start-all          # Starts infra + all backend + frontend
```

### Infrastructure (PostgreSQL & RabbitMQ via Podman)
```bash
make infra-up           # Start containers
make infra-down         # Stop containers
make infra-logs         # View container logs
```

### Backend
```bash
make build-backend      # dotnet build Backend/Proyecto_PetFoundation.sln
make test-backend       # dotnet test Backend/Proyecto_PetFoundation.sln
make backend            # Start all 4 services in background (logs in /tmp/petfoundation-*.log)
make backend-rest       # Start REST API only (port 5001)
make backend-gateway    # Start Gateway only (port 5000)
```

Run a single service with hot-reload:
```bash
dotnet watch --project Backend/src/ApiPetFoundation.Api/ApiPetFoundation.Api.csproj
```

Run a specific test project:
```bash
dotnet test Backend/src/<TestProject>/<TestProject>.csproj
```

### Frontend
```bash
cd Proyecto-PetFoundation && npm install   # First time only
make frontend                              # Start dev server (port 4200)
make test-frontend                         # Run Karma/Jasmine tests
```

### EF Core Migrations
```bash
dotnet ef migrations add <Name> --project Backend/src/ApiPetFoundation.Infrastructure
dotnet ef database update --project Backend/src/ApiPetFoundation.Api
```

### Cleanup
```bash
make clean              # Stop all services, remove logs
pkill -f 'dotnet run'   # Kill background backend services
```

## Architecture

### Services & Ports

| Service | Port | Project |
|---------|------|---------|
| API Gateway (YARP) | 5000 | `Backend/src/ApiPetFoundation.Gateway` |
| REST API | 5001 | `Backend/src/ApiPetFoundation.Api` |
| SOAP API | 5003 | `Backend/src/ApiPetFoundation.Soap.Api` |
| Notifications (SignalR) | 5004 | `Backend/src/ApiPetFoundation.Notifications.Api` |
| Frontend (Angular) | 4200 | `Proyecto-PetFoundation/` |
| PostgreSQL | 5432 | via `compose.yaml` |
| RabbitMQ | 5672/15672 | via `compose.yaml` |

### Request Flow

The **API Gateway** (YARP) routes all external requests:
- `/api/**` → REST API (5001)
- `/soap/**` → SOAP API (5003)
- `/hubs/**` → Notifications API (5004)

The Angular frontend uses a dev proxy (`proxy.conf.json`) to forward requests to the gateway. In production, API URLs are relative paths configured in `Proyecto-PetFoundation/src/app/config/api.config.ts`.

### Backend Clean Architecture

The .NET solution (`Backend/Proyecto_PetFoundation.sln`) follows clean architecture with shared layers:

- **Domain** (`ApiPetFoundation.Domain`) — Entities (`Pet`, `User`, `AdoptionRequest`, `PetImage`, `Notification`), constants. No external dependencies.
- **Application** (`ApiPetFoundation.Application`) — DTOs, service interfaces/implementations, validators (FluentValidation), events, exceptions. References Domain only.
- **Infrastructure** (`ApiPetFoundation.Infrastructure`) — EF Core DbContext + repositories (PostgreSQL/Npgsql), JWT+Identity auth, Supabase storage integration, RabbitMQ event publishing, migrations.
- **API projects** — Thin HTTP hosts that wire up DI and expose endpoints. Each references Application and Infrastructure.

### Messaging (RabbitMQ)

Events are published by the REST API and consumed by the Notifications service:
- Exchange: `petfoundation.events` (topic)
- Queue: `notifications.pets`
- Routing keys: `pet.created`, `pet.updated`, `pet.deleted`, `pet.adopted`

The Notifications API runs a `BackgroundService` that consumes from RabbitMQ and pushes real-time updates to connected Angular clients via a SignalR hub at `/hubs/notifications`.

### Frontend Structure

Angular 17 standalone components (no NgModules). Key directories under `Proyecto-PetFoundation/src/app/`:
- `components/` — Feature components (home, login, register, pet-list, pet-detail, pet-form, adoption-list, notifications, etc.)
- `services/` — HTTP services (auth, pet, adoption-request, notification, signalr, toast)
- `guards/` — `authGuard` (logged in) and `adminGuard` (admin role)
- `interceptors/` — HTTP interceptor for JWT token injection
- `models/` — TypeScript interfaces

Routes are defined in `app.routes.ts`. Admin-only routes (`pets/create`, `pets/:id/edit`) use `adminGuard`.

## Configuration

All backend services read from the `.env` file in the project root (loaded by Makefile). Key variables:
- `ConnectionStrings__DefaultConnection` — PostgreSQL connection string
- `RabbitMq__HostName`, `RabbitMq__Port` — RabbitMQ connection
- `Jwt__Key`, `Jwt__Issuer`, `Jwt__Audience` — JWT settings
- `Supabase__Url`, `Supabase__ServiceRoleKey`, `Supabase__Bucket` — Image storage
- `Seed__AdminEmail`, `Seed__AdminPassword` — Initial admin user

Service-specific settings are in each project's `appsettings.json`. Gateway routing is configured in `ApiPetFoundation.Gateway/appsettings.json`.

## Key Tech Stack

- **Backend**: .NET 10.0, ASP.NET Core, Entity Framework Core 9.0, FluentValidation, YARP 2.2
- **Frontend**: Angular 17, @microsoft/signalr, Karma+Jasmine for tests
- **Infra**: PostgreSQL 16, RabbitMQ 3, Podman/Podman Compose, Supabase (cloud storage)
