# PetFoundation - Microservices Application

A comprehensive pet adoption platform built with microservices architecture using .NET 10.0 and Angular 17.

## 🏗️ Architecture

This project implements a microservices architecture with the following components:

- **Frontend**: Angular 17 with SignalR for real-time notifications
- **API Gateway**: YARP reverse proxy for request routing
- **REST API**: Main business logic and CRUD operations
- **SOAP API**: Legacy SOAP web services
- **Notifications API**: Real-time notifications using SignalR
- **PostgreSQL**: Primary database
- **RabbitMQ**: Message broker for asynchronous communication
- **Supabase**: Cloud storage for images

## 🚀 Quick Start

### Prerequisites
- **Podman and Podman Compose** - For infrastructure services
- **.NET SDK 10.0** - For backend development
- **Node.js 20+** - For frontend development

### Setup

1. **Configure environment variables**
   ```bash
   # Copy the example file and customize
   cp .env.example .env
   # Edit .env with your database password and other settings
   ```

2. **Start infrastructure services (PostgreSQL & RabbitMQ)**
   ```bash
   make infra-up
   ```

3. **Run all backend services**
   ```bash
   make backend
   # This starts all 4 backend services in the background
   # Logs are available in /tmp/petfoundation-*.log
   ```

4. **Run frontend**
   ```bash
   make frontend
   # Or manually:
   # cd Proyecto-PetFoundation && npm install && npm start
   ```

5. **Access the application**
   - Frontend: http://localhost:4200
   - API Gateway: https://localhost:5000
   - REST API Swagger: https://localhost:5001/swagger
   - RabbitMQ Management: http://localhost:15672 (guest/guest)

### Useful Commands

```bash
make help              # Show all available commands
make start-all         # Start infrastructure + backend + frontend
make clean             # Stop all services and clean up
make infra-logs        # View infrastructure logs
```

## 📚 Documentation

- **[Development Guide](./DEV_GUIDE.md)** - Complete setup, running services, troubleshooting
- **[Integration Guide](./INTEGRATION_GUIDE.md)** - Frontend-backend integration, API reference, authentication

## 🛠️ Local Development (without containers)

The easiest way to run services locally is using the Makefile:

```bash
# Start infrastructure
make infra-up

# Start all backend services in background
make backend

# Start frontend
make frontend
```

For manual control or debugging, you can run services individually:

```bash
# Build solution
dotnet build Backend/Proyecto_PetFoundation.sln

# Run individual services in separate terminals
dotnet run --project Backend/src/ApiPetFoundation.Api/ApiPetFoundation.Api.csproj
dotnet run --project Backend/src/ApiPetFoundation.Soap.Api/ApiPetFoundation.Soap.Api.csproj
dotnet run --project Backend/src/ApiPetFoundation.Notifications.Api/ApiPetFoundation.Notifications.Api.csproj
dotnet run --project Backend/src/ApiPetFoundation.Gateway/ApiPetFoundation.Gateway.csproj

# Frontend
cd Proyecto-PetFoundation && npm install && npm start
```

**Note**: Services read configuration from `.env` file. See [Environment Variables](#-environment-variables) section.

## 📦 Services

| Service | Port | Description |
|---------|------|-------------|
| Frontend | 4200 | Angular application |
| API Gateway | 5000 | YARP reverse proxy |
| REST API | 5001 | Main API |
| SOAP API | 5003 | SOAP web service |
| Notifications | 5004 | SignalR hub |
| PostgreSQL | 5432 | Database |
| RabbitMQ | 5672 | Message broker |
| RabbitMQ UI | 15672 | Management interface |

## 🔑 Environment Variables

All configuration is managed through the `.env` file in the project root. Copy `.env.example` to `.env` and customize as needed.

### Required Variables

```bash
# Database connection (override default password)
ConnectionStrings__DefaultConnection=Host=localhost;Port=5432;Database=petfoundationdb;Username=postgres;Password=postgreSQL2025
```

### Optional Configuration

#### Service Ports
All backend services support configurable ports via environment variables:

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

#### Other Settings
```bash
# RabbitMQ
RabbitMq__HostName=localhost
RabbitMq__Port=5672

# JWT
Jwt__Key=your-secret-key-here

# Admin Seed
Seed__AdminEmail=admin@petfoundation.com
Seed__AdminPassword=Admin123!
```

See [.env.example](file:///.env.example) for all available options.

### Frontend Configuration

The frontend API endpoints are configured in [`Proyecto-PetFoundation/src/app/config/api.config.ts`](file:///Proyecto-PetFoundation/src/app/config/api.config.ts):

```typescript
export const API_CONFIG = {
  baseUrl: 'http://localhost:5000/api',
  signalrHubUrl: 'http://localhost:5003/hubs/notifications'
};
```

## 🧪 Testing

```bash
# Run backend tests
dotnet test Backend/Proyecto_PetFoundation.sln

# Run frontend tests
cd Proyecto-PetFoundation
npm test
```

## 📖 API Documentation

- **REST API**: http://localhost:5001/swagger
- **SOAP WSDL**: http://localhost:5003/Service.asmx?wsdl

## 🔄 Message Queue Events

The application uses RabbitMQ for asynchronous communication:

- `pet.created` - Published when a new pet is created
- `pet.updated` - Published when a pet is updated
- `pet.deleted` - Published when a pet is deleted
- `pet.adopted` - Published when a pet is adopted

## 🤝 Contributing

1. Create a feature branch
2. Make your changes
3. Run tests
4. Submit a pull request

## 📝 License

This project is for educational purposes.

## 🆘 Support

For issues and questions:
- Check the [Development Guide](./DEV_GUIDE.md) for troubleshooting
- Review the [Integration Guide](./INTEGRATION_GUIDE.md) for API details

