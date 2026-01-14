import { Component, OnInit } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { CommonModule } from '@angular/common';
import { NavbarComponent } from './components/navbar/navbar.component';
import { ToastComponent } from './components/toast/toast.component';
import { SignalrService } from './services/signalr.service';
import { AuthService } from './services/auth.service';
import { ToastService } from './services/toast.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, CommonModule, NavbarComponent, ToastComponent],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent implements OnInit {
  title = 'Pet Foundation';

  constructor(
    private signalrService: SignalrService,
    private authService: AuthService,
    private toastService: ToastService
  ) {}

  ngOnInit(): void {
    // Iniciar conexión SignalR
    const token = this.authService.getToken();
    this.signalrService.startConnection(token || undefined);

    // Suscribirse a eventos de SignalR y mostrar notificaciones in-app
    this.signalrService.newPetAvailable$.subscribe(data => {
      console.log('🐾 Nueva mascota disponible:', data);
      const petName = data.Pet?.name || 'Una nueva mascota';
      this.toastService.success(
        '🐾 Nueva Mascota Disponible',
        `${petName} está disponible para adopción`
      );
      this.toastService.showBrowserNotification(
        '🐾 Nueva Mascota',
        `${petName} está disponible para adopción`
      );
    });

    this.signalrService.newAdoptionRequest$.subscribe(data => {
      console.log('📝 Nueva solicitud de adopción:', data);
      this.toastService.info(
        '📋 Nueva Solicitud',
        'Hay una nueva solicitud de adopción pendiente'
      );
      this.toastService.showBrowserNotification(
        '📋 Nueva Solicitud',
        'Hay una nueva solicitud de adopción pendiente'
      );
    });

    this.signalrService.adoptionStatusChanged$.subscribe(data => {
      console.log('✅ Estado de adopción cambiado:', data);
      const status = data.AdoptionRequest?.status || 'actualizado';
      this.toastService.success(
        '✅ Estado Actualizado',
        `Tu solicitud ha sido ${status}`
      );
      this.toastService.showBrowserNotification(
        '✅ Estado Actualizado',
        `Tu solicitud ha sido ${status}`
      );
    });

    this.signalrService.connected$.subscribe(connectionId => {
      console.log('🔗 Conectado a SignalR con ID:', connectionId);
    });
  }
}
