# C4 Dynamic Diagram - Adoption Request Flow

This diagram shows the sequence of interactions when a user submits an adoption request and an administrator approves it.

## Adoption Request Submission

```mermaid
C4Dynamic
  title Dynamic Diagram - Adoption Request Submission

  Container(spa, "Angular SPA", "Angular 17", "Pet adoption frontend")
  Container(gateway, "API Gateway", "YARP", "Reverse proxy")

  Container_Boundary(restApi, "REST API") {
    Component(adoptionCtrl, "AdoptionRequestsController", "ASP.NET Core", "Adoption endpoints")
    Component(adoptionSvc, "AdoptionRequestService", "Application", "Adoption business logic")
    Component(adoptionRepo, "AdoptionRequestRepository", "EF Core", "Data access")
    Component(eventPub, "RabbitMqEventPublisher", "RabbitMQ.Client", "Event publishing")
  }

  ContainerDb(postgres, "PostgreSQL", "PostgreSQL 16", "Application data")
  ContainerQueue(rabbitmq, "adoption.request", "RabbitMQ", "Adoption request events")

  Container_Boundary(notifApi, "Notifications Service") {
    Component(subscriber, "RabbitMqPetEventSubscriber", "BackgroundService", "Event consumer")
    Component(notifSvc, "NotificationService", "Application", "Notification persistence")
    Component(hub, "NotificationHub", "SignalR", "WebSocket push")
  }

  Rel(spa, gateway, "1. POST /api/adoptionrequests", "JSON/HTTPS")
  Rel(gateway, adoptionCtrl, "2. Routes request", "HTTP")
  Rel(adoptionCtrl, adoptionSvc, "3. CreateRequestAsync()")
  Rel(adoptionSvc, adoptionRepo, "4. Persist adoption request")
  Rel(adoptionRepo, postgres, "5. INSERT AdoptionRequest", "SQL")
  Rel(adoptionSvc, eventPub, "6. Publish AdoptionRequestCreated")
  Rel(eventPub, rabbitmq, "7. Send event", "AMQP")
  Rel(subscriber, rabbitmq, "8. Consume event", "AMQP")
  Rel(subscriber, notifSvc, "9. Create admin notifications")
  Rel(notifSvc, postgres, "10. INSERT Notification for each admin", "SQL")
  Rel(subscriber, hub, "11. Broadcast NewAdoptionRequest")
  Rel(hub, spa, "12. Push via WebSocket", "WSS")
```

## Adoption Approval

```mermaid
C4Dynamic
  title Dynamic Diagram - Adoption Approval

  Container(spa, "Angular SPA", "Angular 17", "Admin interface")
  Container(gateway, "API Gateway", "YARP", "Reverse proxy")

  Container_Boundary(restApi, "REST API") {
    Component(adoptionCtrl, "AdoptionRequestsController", "ASP.NET Core", "Adoption endpoints")
    Component(adoptionSvc, "AdoptionRequestService", "Application", "Adoption business logic")
    Component(adoptionRepo, "AdoptionRequestRepository", "EF Core", "Data access")
    Component(petRepo, "PetRepository", "EF Core", "Pet data access")
    Component(eventPub, "RabbitMqEventPublisher", "RabbitMQ.Client", "Event publishing")
  }

  ContainerDb(postgres, "PostgreSQL", "PostgreSQL 16", "Application data")
  ContainerQueue(rabbitmq, "adoption.status", "RabbitMQ", "Status change events")

  Container_Boundary(notifApi, "Notifications Service") {
    Component(subscriber, "RabbitMqPetEventSubscriber", "BackgroundService", "Event consumer")
    Component(notifSvc, "NotificationService", "Application", "Notification persistence")
    Component(hub, "NotificationHub", "SignalR", "WebSocket push")
  }

  Rel(spa, gateway, "1. POST /api/adoptionrequests/:id/approve", "JSON/HTTPS")
  Rel(gateway, adoptionCtrl, "2. Routes request", "HTTP")
  Rel(adoptionCtrl, adoptionSvc, "3. ApproveAsync(requestId, adminUserId)")
  Rel(adoptionSvc, adoptionRepo, "4. Update status to Approved")
  Rel(adoptionSvc, petRepo, "5. Update pet status to Adopted")
  Rel(adoptionRepo, postgres, "6. UPDATE AdoptionRequest + Pet", "SQL")
  Rel(adoptionSvc, eventPub, "7. Publish AdoptionStatusChanged")
  Rel(eventPub, rabbitmq, "8. Send event", "AMQP")
  Rel(subscriber, rabbitmq, "9. Consume event", "AMQP")
  Rel(subscriber, notifSvc, "10. Create notification for requester")
  Rel(notifSvc, postgres, "11. INSERT Notification", "SQL")
  Rel(subscriber, hub, "12. Send AdoptionStatusChanged to user")
  Rel(hub, spa, "13. Push via WebSocket", "WSS")
```

## State Machine

The adoption request follows this state machine:

```
                  ┌─────────┐
                  │ Pending │
                  └────┬────┘
           ┌──────────┼──────────┐
           ▼          ▼          ▼
      ┌─────────┐ ┌─────────┐ ┌───────────┐
      │ Approved│ │ Rejected│ │ Cancelled │
      └─────────┘ └─────────┘ └───────────┘
         (admin)    (admin)    (user/admin)
```

| Transition | Actor | Side Effects |
|-----------|-------|-------------|
| Pending -> Approved | Admin | Pet status set to "Adopted", notification sent to requester |
| Pending -> Rejected | Admin | Notification sent to requester |
| Pending -> Cancelled | User or Admin | No pet status change |
