import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { Subject } from 'rxjs';
import { API_CONFIG } from '../config/api.config';

@Injectable({
  providedIn: 'root'
})
export class SignalrService {
  private hubConnection!: signalR.HubConnection;
  
  // Observables para las notificaciones
  public newPetAvailable$ = new Subject<any>();
  public newAdoptionRequest$ = new Subject<any>();
  public adoptionStatusChanged$ = new Subject<any>();
  public connected$ = new Subject<string>();

  constructor() { }

  // Iniciar conexión con SignalR
  public startConnection(token?: string): void {
    const options: signalR.IHttpConnectionOptions = {};

    // Agregar token de autenticación si está disponible
    if (token) {
      options.accessTokenFactory = () => token;
    }

    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(API_CONFIG.signalrHubUrl, options)
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Information)
      .build();

    this.hubConnection
      .start()
      .then(() => {
        console.log('✅ Conexión SignalR establecida');
        this.registerListeners();
      })
      .catch(err => console.error('❌ Error conectando SignalR:', err));
  }

  // Registrar listeners para eventos del servidor
  private registerListeners(): void {
    // Cuando el servidor confirma la conexión
    this.hubConnection.on('Connected', (connectionId: string) => {
      console.log('🔗 Conectado con ID:', connectionId);
      this.connected$.next(connectionId);
    });

    // Cuando hay una nueva mascota disponible
    this.hubConnection.on('PetCreated', (petData: any) => {
      console.log('🐾 Nueva mascota disponible (PetCreated):', petData);
      this.newPetAvailable$.next(petData);
    });

    // También escuchar el evento antiguo por compatibilidad
    this.hubConnection.on('NewPetAvailable', (petData: any) => {
      console.log('🐾 Nueva mascota disponible (NewPetAvailable):', petData);
      this.newPetAvailable$.next(petData);
    });

    // Cuando hay una nueva solicitud de adopción
    this.hubConnection.on('AdoptionRequestCreated', (adoptionData: any) => {
      console.log('📝 Nueva solicitud de adopción (AdoptionRequestCreated):', adoptionData);
      this.newAdoptionRequest$.next(adoptionData);
    });

    // También escuchar el evento antiguo por compatibilidad
    this.hubConnection.on('NewAdoptionRequest', (adoptionData: any) => {
      console.log('📝 Nueva solicitud de adopción (NewAdoptionRequest):', adoptionData);
      this.newAdoptionRequest$.next(adoptionData);
    });

    // Cuando cambia el estado de una adopción
    this.hubConnection.on('AdoptionStatusChanged', (statusData: any) => {
      console.log('✅ Estado de adopción actualizado:', statusData);
      this.adoptionStatusChanged$.next(statusData);
    });
  }

  // Detener conexión
  public stopConnection(): void {
    if (this.hubConnection) {
      this.hubConnection.stop()
        .then(() => console.log('🔌 Conexión SignalR cerrada'))
        .catch(err => console.error('❌ Error cerrando SignalR:', err));
    }
  }

  // Verificar estado de conexión
  public isConnected(): boolean {
    return this.hubConnection?.state === signalR.HubConnectionState.Connected;
  }
}
