.PHONY: help infra infra-up infra-down infra-logs backend-rest backend-soap backend-notifications backend-gateway backend frontend start-backend start-all stop-infra clean

# Default target
help:
	@echo "PetFoundation - Makefile Commands"
	@echo "=================================="
	@echo ""
	@echo "Infrastructure Commands:"
	@echo "  make infra-up          - Start PostgreSQL and RabbitMQ containers"
	@echo "  make infra-down        - Stop infrastructure containers"
	@echo "  make infra-logs        - View infrastructure logs"
	@echo "  make stop-infra        - Stop and remove infrastructure containers"
	@echo ""
	@echo "Backend Commands:"
	@echo "  make backend-rest      - Start REST API service (port 5001)"
	@echo "  make backend-soap      - Start SOAP API service (port 5003)"
	@echo "  make backend-notifications - Start Notifications API service (port 5004)"
	@echo "  make backend-gateway   - Start API Gateway (port 5000)"
	@echo "  make backend           - Start all backend services (in background)"
	@echo "  make start-backend     - Alias for 'make backend'"
	@echo ""
	@echo "Frontend Commands:"
	@echo "  make frontend          - Start Angular frontend (port 4200)"
	@echo ""
	@echo "Combined Commands:"
	@echo "  make start-all         - Start infrastructure + all backend services + frontend"
	@echo ""
	@echo "Utility Commands:"
	@echo "  make clean             - Stop all services and clean up"
	@echo "  make build-backend     - Build all backend projects"
	@echo "  make test-backend      - Run backend tests"
	@echo "  make test-frontend     - Run frontend tests"

# Infrastructure commands
infra-up:
	@echo "🚀 Starting infrastructure services (PostgreSQL & RabbitMQ)..."
	podman-compose up -d
	@echo "✅ Infrastructure services started"
	@echo "   - PostgreSQL: localhost:5432"
	@echo "   - RabbitMQ: localhost:5672"
	@echo "   - RabbitMQ Management UI: http://localhost:15672 (guest/guest)"

infra-down:
	@echo "🛑 Stopping infrastructure services..."
	podman-compose down
	@echo "✅ Infrastructure services stopped"

infra-logs:
	@echo "📋 Showing infrastructure logs..."
	podman-compose logs -f

stop-infra:
	@echo "🛑 Stopping and removing infrastructure containers..."
	podman-compose down -v
	@echo "✅ Infrastructure containers removed"

# Backend service commands
backend-rest:
	@echo "🚀 Starting REST API service..."
	dotnet run --project Backend/src/ApiPetFoundation.Api/ApiPetFoundation.Api.csproj

backend-soap:
	@echo "🚀 Starting SOAP API service..."
	dotnet run --project Backend/src/ApiPetFoundation.Soap.Api/ApiPetFoundation.Soap.Api.csproj

backend-notifications:
	@echo "🚀 Starting Notifications API service..."
	dotnet run --project Backend/src/ApiPetFoundation.Notifications.Api/ApiPetFoundation.Notifications.Api.csproj

backend-gateway:
	@echo "🚀 Starting API Gateway..."
	dotnet run --project Backend/src/ApiPetFoundation.Gateway/ApiPetFoundation.Gateway.csproj

# Include .env file if it exists
-include .env
export

# Start all backend services (requires multiple terminals or use tmux/screen)
backend:
	@echo "⚠️  Note: This will start all backend services in the background."
	@echo "⚠️  Use 'pkill -f dotnet' to stop them, or run individual services in separate terminals."
	@echo ""
	@if [ ! -f .env ]; then \
		echo "⚠️  Warning: .env file not found. Using defaults from appsettings.json"; \
		echo "💡 Tip: Copy .env.example to .env and customize as needed"; \
		echo ""; \
	fi
	@echo "🚀 Starting all backend services..."
	@dotnet run --project Backend/src/ApiPetFoundation.Api/ApiPetFoundation.Api.csproj > /tmp/petfoundation-rest.log 2>&1 & \
	echo "✅ REST API started (PID: $$!) - Logs: /tmp/petfoundation-rest.log"
	@dotnet run --project Backend/src/ApiPetFoundation.Soap.Api/ApiPetFoundation.Soap.Api.csproj > /tmp/petfoundation-soap.log 2>&1 & \
	echo "✅ SOAP API started (PID: $$!) - Logs: /tmp/petfoundation-soap.log"
	@dotnet run --project Backend/src/ApiPetFoundation.Notifications.Api/ApiPetFoundation.Notifications.Api.csproj > /tmp/petfoundation-notifications.log 2>&1 & \
	echo "✅ Notifications API started (PID: $$!) - Logs: /tmp/petfoundation-notifications.log"
	@dotnet run --project Backend/src/ApiPetFoundation.Gateway/ApiPetFoundation.Gateway.csproj > /tmp/petfoundation-gateway.log 2>&1 & \
	echo "✅ Gateway started (PID: $$!) - Logs: /tmp/petfoundation-gateway.log"
	@echo ""
	@echo "✅ All backend services started in background"
	@echo "   - REST API: http://localhost:5001/swagger"
	@echo "   - SOAP API: http://localhost:5003/Service.asmx?wsdl"
	@echo "   - Notifications: http://localhost:5004"
	@echo "   - Gateway: http://localhost:5000"
	@echo ""
	@echo "💡 To stop all services: pkill -f 'dotnet run'"

start-backend: backend

# Frontend commands
frontend:
	@echo "🚀 Starting Angular frontend..."
	@if [ ! -d "Proyecto-PetFoundation/node_modules" ]; then \
		echo "📦 Installing npm dependencies..."; \
		cd Proyecto-PetFoundation && npm install; \
	fi
	@echo "✅ Starting development server..."
	cd Proyecto-PetFoundation && npm start

# Combined command to start everything
start-all:
	@echo "🚀 Starting complete PetFoundation application..."
	@echo ""
	@echo "Step 1/3: Starting infrastructure..."
	@$(MAKE) infra-up
	@echo ""
	@sleep 3
	@echo "Step 2/3: Starting backend services..."
	@$(MAKE) backend
	@echo ""
	@sleep 2
	@echo "Step 3/3: Starting frontend..."
	@echo "⚠️  Frontend will start in this terminal. Open a new terminal to continue working."
	@echo ""
	@$(MAKE) frontend

# Build commands
build-backend:
	@echo "🔨 Building backend solution..."
	dotnet build Backend/Proyecto_PetFoundation.sln
	@echo "✅ Backend build complete"

# Test commands
test-backend:
	@echo "🧪 Running backend tests..."
	dotnet test Backend/Proyecto_PetFoundation.sln
	@echo "✅ Backend tests complete"

test-frontend:
	@echo "🧪 Running frontend tests..."
	cd Proyecto-PetFoundation && npm test
	@echo "✅ Frontend tests complete"

# Clean up
clean:
	@echo "🧹 Cleaning up all services..."
	@echo "Stopping backend services..."
	@-pkill -f 'dotnet run' 2>/dev/null || true
	@echo "Stopping infrastructure..."
	@$(MAKE) infra-down
	@echo "Removing log files..."
	@rm -f /tmp/petfoundation-*.log
	@echo "✅ Cleanup complete"
