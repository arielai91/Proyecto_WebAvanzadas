# Build and run (local)

## Build

```
dotnet build Backend/Proyecto_PetFoundation.sln
```

## Run services

```
dotnet run --project Backend/src/ApiPetFoundation.Api/ApiPetFoundation.Api.csproj
dotnet run --project Backend/src/ApiPetFoundation.Soap.Api/ApiPetFoundation.Soap.Api.csproj
dotnet run --project Backend/src/ApiPetFoundation.Notifications.Api/ApiPetFoundation.Notifications.Api.csproj
dotnet run --project Backend/src/ApiPetFoundation.Gateway/ApiPetFoundation.Gateway.csproj
```
