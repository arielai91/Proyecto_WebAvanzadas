# C4 Level 3 - Component Diagram: Angular SPA

This diagram shows the internal components of the Angular frontend application.

```mermaid
C4Component
  title Component Diagram - Angular SPA (Proyecto-PetFoundation)

  Container(gateway, "API Gateway", "YARP", "Backend entry point")

  Container_Boundary(spa, "Angular SPA") {

    Component(router, "App Router", "Angular Router", "Defines routes and applies guards")
    Component(authGuard, "Auth Guard", "Angular Guard", "Protects authenticated routes")
    Component(adminGuard, "Admin Guard", "Angular Guard", "Protects admin-only routes")
    Component(authInterceptor, "Auth Interceptor", "HTTP Interceptor", "Injects JWT token in requests")

    Component(homeComp, "HomeComponent", "Angular 17", "Landing page with featured pets")
    Component(petListComp, "PetListComponent", "Angular 17", "Pet browsing with filters and QR scanner")
    Component(petDetailComp, "PetDetailComponent", "Angular 17", "Pet details, QR generation, adoption form")
    Component(petFormComp, "PetFormComponent", "Angular 17", "Pet create/edit form with image upload")
    Component(adoptionListComp, "AdoptionListComponent", "Angular 17", "Adoption request management")
    Component(notifComp, "NotificationsComponent", "Angular 17", "Notification list with filtering")
    Component(navbarComp, "NavbarComponent", "Angular 17", "Navigation, user menu, notification badge")

    Component(authService, "AuthService", "Angular Service", "Login, register, token management")
    Component(petService, "PetService", "Angular Service", "Pet CRUD and image upload")
    Component(adoptionService, "AdoptionRequestService", "Angular Service", "Adoption workflow API calls")
    Component(notifService, "NotificationService", "Angular Service", "Notification retrieval and read status")
    Component(signalrService, "SignalrService", "Angular Service", "Real-time WebSocket connection")
    Component(toastService, "ToastService", "Angular Service", "In-app and browser notifications")

  }

  Rel(router, authGuard, "Applies to protected routes")
  Rel(router, adminGuard, "Applies to admin routes")

  Rel(homeComp, petService, "Loads featured pets")
  Rel(homeComp, signalrService, "Subscribes to new pets")
  Rel(petListComp, petService, "Queries with filters")
  Rel(petDetailComp, petService, "Loads pet details")
  Rel(petDetailComp, adoptionService, "Submits adoption request")
  Rel(petFormComp, petService, "Creates/updates pets")
  Rel(adoptionListComp, adoptionService, "Lists and manages requests")
  Rel(notifComp, notifService, "Loads notifications")
  Rel(navbarComp, authService, "Checks auth state")
  Rel(navbarComp, notifService, "Shows unread count")

  Rel(signalrService, gateway, "Connects via WebSocket", "WSS")
  Rel(authService, gateway, "POST /api/auth/*", "JSON/HTTPS")
  Rel(petService, gateway, "CRUD /api/pets", "JSON/HTTPS")
  Rel(adoptionService, gateway, "CRUD /api/adoptionrequests", "JSON/HTTPS")
  Rel(notifService, gateway, "GET/POST /api/notifications", "JSON/HTTPS")
  Rel(authInterceptor, authService, "Gets JWT token")

  Rel(signalrService, toastService, "Triggers notifications")

  UpdateLayoutConfig($c4ShapeInRow="4", $c4BoundaryInRow="1")
```

## Frontend Architecture

### Routing & Guards

| Route | Component | Guard | Access |
|-------|-----------|-------|--------|
| `/` | HomeComponent | None | Public |
| `/login` | LoginComponent | None | Public |
| `/register` | RegisterComponent | None | Public |
| `/pets` | PetListComponent | None | Public |
| `/pets/create` | PetFormComponent | adminGuard | Admin only |
| `/pets/:id` | PetDetailComponent | None | Public |
| `/pets/:id/edit` | PetFormComponent | adminGuard | Admin only |
| `/adoption-requests` | AdoptionListComponent | authGuard | Authenticated |
| `/notifications` | NotificationsComponent | authGuard | Authenticated |

### Services

| Service | State Management | API Endpoints |
|---------|-----------------|---------------|
| **AuthService** | BehaviorSubject for currentUser, localStorage for token | `/api/auth/login`, `/api/auth/register`, `/api/auth/me` |
| **PetService** | Stateless (per-request) | `/api/pets/**` |
| **AdoptionRequestService** | Stateless (per-request) | `/api/adoptionrequests/**` |
| **NotificationService** | Stateless (per-request) | `/api/notifications/**` |
| **SignalrService** | RxJS Subjects for real-time events | WebSocket `/hubs/notifications` |
| **ToastService** | Observable toast queue | N/A (UI only) |

### Real-Time Flow

```
RabbitMQ → Notifications Service → SignalR Hub → WebSocket → SignalrService → Components
                                                                           → ToastService → Browser Notification API
```
