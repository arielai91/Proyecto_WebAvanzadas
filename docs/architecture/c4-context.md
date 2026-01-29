# C4 Level 1 - System Context Diagram

This diagram shows the PetFoundation system and its interactions with external actors and systems.

```mermaid
C4Context
  title System Context - PetFoundation

  Person(adopter, "Adopter", "User who browses pets and submits adoption requests")
  Person(admin, "Administrator", "Manages pets, approves or rejects adoption requests")

  System(petfoundation, "PetFoundation Platform", "Pet adoption platform with real-time notifications, REST/SOAP APIs, and QR-based pet lookup")

  System_Ext(supabase, "Supabase Storage", "Cloud object storage for pet images")
  System_Ext(browser, "Web Browser", "Renders Angular SPA, provides camera and notification APIs")

  Rel(adopter, petfoundation, "Browses pets, submits adoption requests, receives notifications")
  Rel(admin, petfoundation, "Creates/edits pets, manages adoption requests")
  Rel(petfoundation, supabase, "Uploads and serves pet images", "HTTPS")
  Rel(adopter, browser, "Accesses platform via")
  Rel(admin, browser, "Accesses platform via")
  Rel(browser, petfoundation, "Loads SPA and makes API calls", "HTTPS/WSS")
```

## Key Actors

| Actor | Role | Key Actions |
|-------|------|-------------|
| **Adopter** | Registered user | Browse pets, view details via QR, submit adoption requests, receive real-time notifications |
| **Administrator** | Admin role | Create/edit/delete pets, upload images, approve/reject adoption requests |

## External Systems

| System | Purpose | Integration |
|--------|---------|-------------|
| **Supabase Storage** | Cloud storage for pet images | REST API with Service Role Key authentication |
| **Web Browser** | Client runtime | Renders Angular SPA, provides Camera API (QR scanning) and Notification API (browser alerts) |
