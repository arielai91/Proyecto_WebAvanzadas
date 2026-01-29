# C4 Level 2 - Container Diagram

This diagram shows the deployable containers that compose the PetFoundation platform.

```mermaid
C4Container
  title Container Diagram - PetFoundation

  Person(adopter, "Adopter", "Browses pets and adopts")
  Person(admin, "Administrator", "Manages pets and requests")

  System_Boundary(petfoundation, "PetFoundation Platform") {

    Container(spa, "Angular SPA", "Angular 17, TypeScript", "Single-page app with real-time updates and QR scanning")

    Container(gateway, "API Gateway", "ASP.NET Core, YARP 2.2", "Reverse proxy routing requests to backend services")

    Container(restApi, "REST API", "ASP.NET Core, .NET 8.0", "Pet CRUD, adoption requests, auth, image upload")

    Container(soapApi, "SOAP API", "ASP.NET Core, SoapCore", "Legacy SOAP interface for pet queries and adoption summaries")

    Container(notificationsApi, "Notifications Service", "ASP.NET Core, SignalR", "Real-time push notifications via WebSocket and event consumption")

    ContainerDb(postgres, "PostgreSQL", "PostgreSQL 16", "Stores users, pets, adoption requests, notifications")

    ContainerQueue(rabbitmq_pet_created, "pet.created", "RabbitMQ", "Pet creation events")
    ContainerQueue(rabbitmq_adoption_status, "adoption.status", "RabbitMQ", "Adoption status change events")
    ContainerQueue(rabbitmq_adoption_request, "adoption.request", "RabbitMQ", "New adoption request events")

  }

  System_Ext(supabase, "Supabase Storage", "Cloud image storage")

  Rel(adopter, spa, "Uses", "HTTPS")
  Rel(admin, spa, "Uses", "HTTPS")

  Rel(spa, gateway, "Sends API requests", "JSON/HTTPS")
  Rel(spa, gateway, "Connects for real-time updates", "WSS")

  Rel(gateway, restApi, "Routes /api/**", "HTTP")
  Rel(gateway, soapApi, "Routes /soap/**", "HTTP")
  Rel(gateway, notificationsApi, "Routes /hubs/**", "WSS")

  Rel(restApi, postgres, "Reads/writes", "EF Core/Npgsql")
  Rel(soapApi, postgres, "Reads", "EF Core/Npgsql")
  Rel(notificationsApi, postgres, "Writes notifications", "EF Core/Npgsql")

  Rel(restApi, rabbitmq_pet_created, "Publishes to", "AMQP")
  Rel(restApi, rabbitmq_adoption_status, "Publishes to", "AMQP")
  Rel(restApi, rabbitmq_adoption_request, "Publishes to", "AMQP")

  Rel(notificationsApi, rabbitmq_pet_created, "Subscribes to", "AMQP")
  Rel(notificationsApi, rabbitmq_adoption_status, "Subscribes to", "AMQP")
  Rel(notificationsApi, rabbitmq_adoption_request, "Subscribes to", "AMQP")

  Rel(restApi, supabase, "Uploads pet images", "HTTPS")

  UpdateLayoutConfig($c4ShapeInRow="3", $c4BoundaryInRow="1")
```

## Container Descriptions

| Container | Technology | Port | Purpose |
|-----------|-----------|------|---------|
| **Angular SPA** | Angular 17, TypeScript, SignalR client | 4200 (dev) | Frontend with reactive UI, QR scanning, real-time notifications |
| **API Gateway** | ASP.NET Core, YARP | 5000 | Reverse proxy routing all external traffic to internal services |
| **REST API** | ASP.NET Core, EF Core, FluentValidation | 5001 | Business logic: pet management, adoption workflow, auth, image upload |
| **SOAP API** | ASP.NET Core, SoapCore | 5003 | Legacy interface for pet queries and adoption summary reports |
| **Notifications Service** | ASP.NET Core, SignalR, RabbitMQ consumer | 5004 | Consumes domain events, persists notifications, pushes real-time updates |
| **PostgreSQL** | PostgreSQL 16 | 5432 | Primary datastore with `identity` and `app` schemas |
| **RabbitMQ** | RabbitMQ 3 | 5672 | Event broker with `petfoundation.events` exchange |

## Request Flow

```
Browser → API Gateway (5000)
            ├── /api/**  → REST API (5001) → PostgreSQL + Supabase + RabbitMQ
            ├── /soap/** → SOAP API (5003) → PostgreSQL
            └── /hubs/** → Notifications (5004) ← RabbitMQ → SignalR → Browser
```
