# PetFoundation Development Guide

This guide provides instructions for setting up and running the PetFoundation application with infrastructure services (PostgreSQL, RabbitMQ) running in Podman containers and application services (.NET, Angular) running locally for optimal development experience.

## Table of Contents
- [Prerequisites](#prerequisites)
- [Quick Start](#quick-start)
- [Infrastructure Services](#infrastructure-services)
- [Running Backend Services](#running-backend-services)
- [Running Frontend](#running-frontend)
- [Development Workflow](#development-workflow)
- [Service URLs](#service-urls)
- [Troubleshooting](#troubleshooting)
- [Useful Commands](#useful-commands)

## Prerequisites

### Required Software
- **Podman** (v4.0+) - Container runtime for infrastructure
- **Podman Compose** (v1.0+) - Multi-container orchestration
- **.NET SDK 10.0** - For backend development
- **Node.js 20+** - For frontend development

### Installation

#### macOS
```bash
# Install Podman
brew install podman

# Install Podman Compose
brew install podman-compose

# Initialize and start Podman machine
podman machine init
podman machine start

# Install .NET SDK 10.0
brew install dotnet@10

# Install Node.js
brew install node@20
```

#### Linux
```bash
# Install Podman (Fedora/RHEL)
sudo dnf install podman podman-compose

# Install Podman (Ubuntu/Debian)
sudo apt-get install podman podman-compose

# Install .NET SDK 10.0
# Follow instructions at: https://dotnet.microsoft.com/download

# Install Node.js
# Follow instructions at: https://nodejs.org/
```

## Quick Start

### Using Makefile (Recommended)

The easiest way to get started is using the provided Makefile:

1. **Configure environment**
   ```bash
   cd /Users/alexpadilla/projects/webapps/Proyecto_WebAvanzadas
   cp .env.example .env
   # Edit .env with your database password (default: postgreSQL2025)
   ```

2. **Start everything**
   ```bash
   make start-all
   ```
   
   This will:
   - Start infrastructure (PostgreSQL & RabbitMQ)
   - Start all 4 backend services in background
   - Start frontend (in foreground)

3. **Access the application**
   - Frontend: http://localhost:4200
   - API Gateway: https://localhost:5000
   - REST API Swagger: https://localhost:5001/swagger
   - RabbitMQ Management: http://localhost:15672 (guest/guest)

**Useful Makefile Commands:**
```bash
make help              # Show all available commands
make infra-up          # Start only infrastructure
make backend           # Start only backend services
make frontend          # Start only frontend
make clean             # Stop all services and clean up
```

### Manual Setup (Alternative)

If you prefer manual control:

1. **Navigate to project directory**
   ```bash
   cd /Users/alexpadilla/projects/webapps/Proyecto_WebAvanzadas
   ```

2. **Configure environment**
   ```bash
   cp .env.example .env
   # Edit .env with your configuration
   ```

3. **Start infrastructure services**
   ```bash
   make infra-up
   # Or manually: podman-compose up -d
   ```
   
   This starts PostgreSQL and RabbitMQ in containers.

4. **Run backend services** (in separate terminals)
   ```bash
   # Terminal 1 - REST API
   dotnet run --project Backend/src/ApiPetFoundation.Api/ApiPetFoundation.Api.csproj
   
   # Terminal 2 - SOAP API
   dotnet run --project Backend/src/ApiPetFoundation.Soap.Api/ApiPetFoundation.Soap.Api.csproj
   
   # Terminal 3 - Notifications API
   dotnet run --project Backend/src/ApiPetFoundation.Notifications.Api/ApiPetFoundation.Notifications.Api.csproj
   
   # Terminal 4 - API Gateway
   dotnet run --project Backend/src/ApiPetFoundation.Gateway/ApiPetFoundation.Gateway.csproj
   ```

5. **Run frontend** (in a new terminal)
   ```bash
   cd Proyecto-PetFoundation
   npm install
   npm start
   ```

6. **Access the application**
   - Frontend: http://localhost:4200
   - API Gateway: https://localhost:5000
   - REST API Swagger: https://localhost:5001/swagger
   - RabbitMQ Management: http://localhost:15672 (guest/guest)

## Environment Configuration

All backend services read configuration from the `.env` file in the project root.

### Required Configuration

```bash
# Database connection (override default password)
ConnectionStrings__DefaultConnection=Host=localhost;Port=5432;Database=petfoundationdb;Username=postgres;Password=postgreSQL2025
```

### Optional Port Configuration

All service ports are configurable via environment variables:

```bash
# Gateway (default: 5000)
ASPNETCORE_HTTPS_PORT=5000

# REST API (default: 5001)
ASPNETCORE_HTTPS_PORT=5001

# SOAP API (default: 5003)
ASPNETCORE_HTTPS_PORT=5003

# Notifications API (defaults: 5003 HTTP, 5004 HTTPS)
ASPNETCORE_HTTP_PORT=5003
ASPNETCORE_HTTPS_PORT=5004
```

See `.env.example` for all available configuration options including RabbitMQ, JWT, and seed data settings.

## Infrastructure Services

The `compose.yaml` file defines only the infrastructure services needed for development:

- **PostgreSQL** - Database on port 5432
- **RabbitMQ** - Message broker on port 5672 (Management UI on 15672)

### Start Infrastructure
```bash
podman-compose up -d
```

### Stop Infrastructure
```bash
podman-compose down
```

### Stop and Remove Data (WARNING: deletes database)
```bash
podman-compose down -v
```

### View Infrastructure Logs
```bash
# All services
podman-compose logs -f

# Specific service
podman-compose logs -f postgres
podman-compose logs -f rabbitmq
```

## Running Backend Services

All .NET backend services run locally for better debugging and hot-reload support.

### Prerequisites
Ensure infrastructure services are running:
```bash
podman-compose ps
```

### Start All Backend Services

Open 4 separate terminal windows/tabs:

**Terminal 1 - REST API (Port 5001)**
```bash
cd /Users/alexpadilla/projects/webapps/Proyecto_WebAvanzadas
dotnet run --project Backend/src/ApiPetFoundation.Api/ApiPetFoundation.Api.csproj
```

**Terminal 2 - SOAP API (Port 5003)**
```bash
cd /Users/alexpadilla/projects/webapps/Proyecto_WebAvanzadas
dotnet run --project Backend/src/ApiPetFoundation.Soap.Api/ApiPetFoundation.Soap.Api.csproj
```

**Terminal 3 - Notifications API (Port 5004)**
```bash
cd /Users/alexpadilla/projects/webapps/Proyecto_WebAvanzadas
dotnet run --project Backend/src/ApiPetFoundation.Notifications.Api/ApiPetFoundation.Notifications.Api.csproj
```

**Terminal 4 - API Gateway (Port 5000)**
```bash
cd /Users/alexpadilla/projects/webapps/Proyecto_WebAvanzadas
dotnet run --project Backend/src/ApiPetFoundation.Gateway/ApiPetFoundation.Gateway.csproj
```

### Build Backend
```bash
dotnet build Backend/Proyecto_PetFoundation.sln
```

### Run Tests
```bash
dotnet test Backend/Proyecto_PetFoundation.sln
```

### Watch Mode (Auto-rebuild on changes)
```bash
dotnet watch --project Backend/src/ApiPetFoundation.Api/ApiPetFoundation.Api.csproj
```

## Running Frontend

The Angular frontend runs locally with hot-reload support.

### First Time Setup
```bash
cd Proyecto-PetFoundation
npm install
```

### Start Development Server
```bash
npm start
```

The frontend will be available at http://localhost:4200 with hot-reload enabled.

### Build for Production
```bash
npm run build
```

### Run Tests
```bash
npm test
```

## Development Workflow

### Recommended Startup Order

1. **Start Infrastructure** (once per development session)
   ```bash
   podman-compose up -d
   ```

2. **Start Backend Services** (in separate terminals)
   - Start services in any order, but Gateway should be last
   - Use `dotnet watch` for hot-reload during development

3. **Start Frontend**
   ```bash
   cd Proyecto-PetFoundation
   npm start
   ```

### Daily Development Flow

**Morning Setup:**
```bash
# Check if infrastructure is running
podman-compose ps

# If not running, start it
podman-compose up -d

# Start your backend services in watch mode
dotnet watch --project Backend/src/ApiPetFoundation.Api/ApiPetFoundation.Api.csproj

# Start frontend
cd Proyecto-PetFoundation && npm start
```

**End of Day:**
```bash
# Stop .NET services: Ctrl+C in each terminal

# Stop frontend: Ctrl+C

# Optionally stop infrastructure (or leave running)
podman-compose down
```

### Hot Reload

**Backend (.NET):**
- Use `dotnet watch` instead of `dotnet run` for automatic rebuild on file changes
- Changes to C# files will trigger automatic recompilation

**Frontend (Angular):**
- `npm start` includes hot-reload by default
- Changes to TypeScript/HTML/CSS files will automatically refresh the browser

### Database Migrations

Run migrations from your local machine:
```bash
cd Backend/src/ApiPetFoundation.Api
dotnet ef database update
```

## Service URLs

| Service | URL | Description |
|---------|-----|-------------|
| **Frontend** | http://localhost:4200 | Angular application (local) |
| **API Gateway** | http://localhost:5000 | YARP reverse proxy (local) |
| **REST API** | http://localhost:5001 | Main REST API (local) |
| **SOAP API** | http://localhost:5003 | SOAP web service (local) |
| **Notifications** | http://localhost:5004 | SignalR hub (local) |
| **PostgreSQL** | localhost:5432 | Database (container) |
| **RabbitMQ** | localhost:5672 | Message broker (container) |
| **RabbitMQ UI** | http://localhost:15672 | Management interface (container) |

### API Documentation

- **REST API Swagger**: http://localhost:5001/swagger
- **SOAP WSDL**: http://localhost:5003/Service.asmx?wsdl

## Troubleshooting

### Infrastructure Services Won't Start

**Check Podman machine status:**
```bash
podman machine list
podman machine start  # if stopped
```

**Check port conflicts:**
```bash
# Check if ports are already in use
lsof -i :5432  # PostgreSQL
lsof -i :5672  # RabbitMQ
```

**View container logs:**
```bash
podman-compose logs postgres
podman-compose logs rabbitmq
```

### Backend Services Won't Start

**Check if infrastructure is running:**
```bash
podman-compose ps
```

**Check .NET SDK version:**
```bash
dotnet --version  # Should be 10.0.x
```

**Database connection errors:**
- Ensure PostgreSQL container is running: `podman ps | grep postgres`
- Test connection: `podman exec -it petfoundation-postgres pg_isready -U postgres`

**RabbitMQ connection errors:**
- Ensure RabbitMQ container is running: `podman ps | grep rabbitmq`
- Check RabbitMQ status: `podman exec -it petfoundation-rabbitmq rabbitmq-diagnostics status`

**Port already in use:**
```bash
# Find process using the port
lsof -i :5001  # or :5003, :5004, :5000

# Kill the process if needed
kill -9 <PID>
```

### Frontend Won't Start

**Node modules issues:**
```bash
cd Proyecto-PetFoundation
rm -rf node_modules package-lock.json
npm install
```

**Port 4200 already in use:**
```bash
# Find and kill the process
lsof -i :4200
kill -9 <PID>

# Or use a different port
npm start -- --port 4201
```

## Useful Commands

### Infrastructure Management

```bash
# List running containers
podman ps

# View container logs
podman logs petfoundation-postgres
podman logs petfoundation-rabbitmq

# Execute command in container
podman exec -it petfoundation-postgres bash
podman exec -it petfoundation-rabbitmq bash

# Restart a container
podman restart petfoundation-postgres
podman restart petfoundation-rabbitmq

# View resource usage
podman stats
```

### Database Operations

```bash
# Connect to PostgreSQL
podman exec -it petfoundation-postgres psql -U postgres -d petfoundationdb

# Create database backup
podman exec petfoundation-postgres pg_dump -U postgres petfoundationdb > backup.sql

# Restore database
cat backup.sql | podman exec -i petfoundation-postgres psql -U postgres petfoundationdb

# Run SQL query
podman exec -it petfoundation-postgres psql -U postgres -d petfoundationdb -c "SELECT * FROM pets;"

# Check database size
podman exec -it petfoundation-postgres psql -U postgres -c "\l+"
```

### RabbitMQ Operations

```bash
# List queues
podman exec petfoundation-rabbitmq rabbitmqctl list_queues

# List exchanges
podman exec petfoundation-rabbitmq rabbitmqctl list_exchanges

# List bindings
podman exec petfoundation-rabbitmq rabbitmqctl list_bindings

# Purge a queue
podman exec petfoundation-rabbitmq rabbitmqctl purge_queue notifications.pets
```

### .NET Development Commands

```bash
# Clean solution
dotnet clean Backend/Proyecto_PetFoundation.sln

# Restore packages
dotnet restore Backend/Proyecto_PetFoundation.sln

# Build specific project
dotnet build Backend/src/ApiPetFoundation.Api/ApiPetFoundation.Api.csproj

# Run with specific environment
ASPNETCORE_ENVIRONMENT=Production dotnet run --project Backend/src/ApiPetFoundation.Api/ApiPetFoundation.Api.csproj

# Entity Framework migrations
dotnet ef migrations add MigrationName --project Backend/src/ApiPetFoundation.Infrastructure
dotnet ef database update --project Backend/src/ApiPetFoundation.Api

# List available migrations
dotnet ef migrations list --project Backend/src/ApiPetFoundation.Api
```

### Angular Development Commands

```bash
# Generate component
cd Proyecto-PetFoundation
ng generate component components/pet-list

# Generate service
ng generate service services/pet

# Run linter
ng lint

# Build for production
ng build --configuration production

# Serve with specific configuration
ng serve --configuration development --port 4200
```

### Volume Management

```bash
# List volumes
podman volume ls

# Inspect volume
podman volume inspect proyecto_webavanzadas_postgres_data

# Remove unused volumes (WARNING: deletes data)
podman volume prune
```

### System Cleanup

```bash
# Remove stopped containers
podman container prune

# Remove unused images
podman image prune

# Remove all unused resources
podman system prune -a

# View disk usage
podman system df
```

## Performance Tips

1. **Use `dotnet watch` for development**: Automatic rebuild on file changes saves time
   ```bash
   dotnet watch --project Backend/src/ApiPetFoundation.Api/ApiPetFoundation.Api.csproj
   ```

2. **Keep infrastructure running**: Leave PostgreSQL and RabbitMQ containers running between sessions to avoid startup time

3. **Use IDE debugging**: Run services from your IDE (Visual Studio, Rider, VS Code) for better debugging experience

4. **Optimize database queries**: Use Entity Framework query logging to identify slow queries
   ```bash
   # Enable in appsettings.Development.json
   "Logging": {
     "LogLevel": {
       "Microsoft.EntityFrameworkCore.Database.Command": "Information"
     }
   }
   ```

5. **Monitor RabbitMQ**: Use the management UI at http://localhost:15672 to monitor message queues

6. **Clean build artifacts**: Periodically clean build artifacts to free disk space
   ```bash
   dotnet clean Backend/Proyecto_PetFoundation.sln
   cd Proyecto-PetFoundation && rm -rf node_modules dist
   ```

## Next Steps

- See [INTEGRATION_GUIDE.md](./INTEGRATION_GUIDE.md) for frontend-backend integration details
- Review API documentation at http://localhost:5001/swagger
- Explore RabbitMQ management UI at http://localhost:15672
- Set up your IDE for debugging .NET services
- Configure environment-specific settings in `appsettings.Development.json`
