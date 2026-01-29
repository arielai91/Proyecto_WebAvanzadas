# C4 Level 4 - Deployment Diagram

This diagram shows how the PetFoundation containers are deployed in the development/production environment.

```mermaid
C4Deployment
  title Deployment Diagram - PetFoundation (Development)

  Deployment_Node(browser, "User Browser", "Chrome/Firefox/Safari") {
    Container(spa, "Angular SPA", "Angular 17", "Pet adoption frontend with real-time updates")
  }

  Deployment_Node(host, "Application Server", "Linux/macOS") {

    Deployment_Node(dotnet, ".NET Runtime", ".NET 8.0") {
      Container(gateway, "API Gateway", "YARP 2.2", "Port 5000 - Reverse proxy")
      Container(restApi, "REST API", "ASP.NET Core", "Port 5001 - Business logic")
      Container(soapApi, "SOAP API", "SoapCore", "Port 5003 - Legacy interface")
      Container(notifApi, "Notifications Service", "SignalR", "Port 5004 - Real-time push")
    }

    Deployment_Node(podman, "Podman Containers", "Podman Compose") {
      ContainerDb(postgres, "PostgreSQL", "PostgreSQL 16 Alpine", "Port 5432 - petfoundationdb")
      ContainerQueue(rabbitmq, "RabbitMQ", "RabbitMQ 3 Management", "Ports 5672/15672")
    }

  }

  Deployment_Node(cloud, "Supabase Cloud", "Supabase Platform") {
    Container(storage, "Object Storage", "Supabase Storage", "Pet image bucket")
  }

  Rel(spa, gateway, "API calls", "HTTPS/WSS")
  Rel(gateway, restApi, "Routes /api/**", "HTTP")
  Rel(gateway, soapApi, "Routes /soap/**", "HTTP")
  Rel(gateway, notifApi, "Routes /hubs/**", "HTTP/WSS")
  Rel(restApi, postgres, "Reads/writes", "TCP/5432")
  Rel(soapApi, postgres, "Reads", "TCP/5432")
  Rel(notifApi, postgres, "Writes notifications", "TCP/5432")
  Rel(restApi, rabbitmq, "Publishes events", "AMQP/5672")
  Rel(notifApi, rabbitmq, "Consumes events", "AMQP/5672")
  Rel(restApi, storage, "Uploads images", "HTTPS")
```

## Port Mapping

| Service | Port | Protocol | Notes |
|---------|------|----------|-------|
| Angular Dev Server | 4200 | HTTPS | Proxies to Gateway |
| API Gateway (YARP) | 5000 | HTTPS | Single entry point for all backend services |
| REST API | 5001 | HTTPS | Internal, accessed via Gateway |
| SOAP API | 5003 | HTTPS | Internal, accessed via Gateway |
| Notifications API | 5004 | HTTPS/WSS | Internal, accessed via Gateway |
| PostgreSQL | 5432 | TCP | Container: petfoundation-postgres |
| RabbitMQ (AMQP) | 5672 | TCP | Container: petfoundation-rabbitmq |
| RabbitMQ (Management) | 15672 | HTTP | Admin UI for queue monitoring |

## Infrastructure as Code

Infrastructure services are defined in `compose.yaml` and managed via Makefile:

```
make infra-up       # Start PostgreSQL + RabbitMQ containers
make backend        # Start all 4 .NET services
make frontend       # Start Angular dev server
make start-all      # Start everything
```

## Database Schemas

PostgreSQL uses two schemas:

| Schema | Tables | Purpose |
|--------|--------|---------|
| `identity` | AspNetUsers, AspNetRoles, AspNetUserRoles, etc. | ASP.NET Core Identity |
| `app` | Users, Pets, PetImages, AdoptionRequests, Notifications | Domain data |
