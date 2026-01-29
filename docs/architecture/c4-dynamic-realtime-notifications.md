# C4 Dynamic Diagram - Real-Time Notification Flow

This diagram shows how domain events flow from the REST API through RabbitMQ to connected clients via SignalR.

## New Pet Created - Notification Flow

```mermaid
C4Dynamic
  title Dynamic Diagram - Pet Created Notification Flow

  Container(spa, "Angular SPA", "Angular 17", "Connected clients")

  Container_Boundary(restApi, "REST API") {
    Component(petsCtrl, "PetsController", "ASP.NET Core", "POST /api/pets")
    Component(petSvc, "PetService", "Application", "Pet business logic")
    Component(eventPub, "RabbitMqEventPublisher", "RabbitMQ.Client", "Event publishing")
  }

  ContainerDb(postgres, "PostgreSQL", "PostgreSQL 16", "Application data")
  ContainerQueue(rabbitmq, "pet.created", "RabbitMQ", "Pet creation events")

  Container_Boundary(notifApi, "Notifications Service") {
    Component(subscriber, "RabbitMqPetEventSubscriber", "BackgroundService", "Event consumer")
    Component(notifSvc, "NotificationService", "Application", "Creates notifications for all users")
    Component(hub, "NotificationHub", "SignalR", "Broadcasts to all connected clients")
  }

  Rel(petsCtrl, petSvc, "1. AddPetAsync()")
  Rel(petSvc, postgres, "2. INSERT Pet", "SQL")
  Rel(petSvc, eventPub, "3. Publish PetCreated event")
  Rel(eventPub, rabbitmq, "4. Send to exchange", "AMQP")
  Rel(subscriber, rabbitmq, "5. Consume from queue", "AMQP")
  Rel(subscriber, notifSvc, "6. Create NEW_PET notification for each user")
  Rel(notifSvc, postgres, "7. INSERT Notifications", "SQL")
  Rel(subscriber, hub, "8. Broadcast NewPetAvailable to all clients")
  Rel(hub, spa, "9. Push via WebSocket", "WSS")
```

## Frontend Real-Time Handling

```mermaid
C4Dynamic
  title Dynamic Diagram - Frontend Notification Handling

  Container(hub, "SignalR Hub", "Notifications Service", "Server-side WebSocket endpoint")

  Container_Boundary(spa, "Angular SPA") {
    Component(signalrSvc, "SignalrService", "Angular Service", "WebSocket connection manager")
    Component(appComp, "AppComponent", "Root Component", "Subscribes to all real-time events")
    Component(toastSvc, "ToastService", "Angular Service", "Shows in-app and browser notifications")
    Component(homeComp, "HomeComponent", "Angular Component", "Reloads featured pets")
    Component(navbarComp, "NavbarComponent", "Angular Component", "Updates notification badge")
  }

  Rel(hub, signalrSvc, "1. Receive NewPetAvailable event", "WSS")
  Rel(signalrSvc, appComp, "2. Emit via newPetAvailable$ Observable")
  Rel(appComp, toastSvc, "3. Show success toast")
  Rel(toastSvc, appComp, "4. Trigger browser notification", "Notification API")
  Rel(signalrSvc, homeComp, "5. Emit triggers pet list reload")
  Rel(signalrSvc, navbarComp, "6. Emit triggers unread count refresh")
```

## Event Types

| Event | Published By | Consumed By | Notification Type | Recipients | SignalR Method |
|-------|-------------|-------------|-------------------|-----------|----------------|
| PetCreated | PetService | Subscriber | NEW_PET | All users | NewPetAvailable |
| AdoptionRequestCreated | AdoptionRequestService | Subscriber | NEW_REQUEST | All admins | NewAdoptionRequest |
| AdoptionStatusChanged | AdoptionRequestService | Subscriber | ADOPTION_STATUS | Requester only | AdoptionStatusChanged |

## RabbitMQ Topology

```
REST API
   │
   ▼
petfoundation.events (Fanout Exchange)
   │
   ▼
notifications.pets (Queue)
   │
   ▼
Notifications Service (BackgroundService consumer)
```
