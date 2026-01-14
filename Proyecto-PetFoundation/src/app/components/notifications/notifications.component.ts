import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { NotificationService } from '../../services/notification.service';
import { SignalrService } from '../../services/signalr.service';
import { Notification } from '../../models/models';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-notifications',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './notifications.component.html',
  styleUrl: './notifications.component.css'
})
export class NotificationsComponent implements OnInit, OnDestroy {
  notifications: Notification[] = [];
  unreadCount = 0;
  isLoading = true;
  
  private subscriptions: Subscription[] = [];

  constructor(
    private notificationService: NotificationService,
    private signalrService: SignalrService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadNotifications();
    this.subscribeToSignalR();
  }

  ngOnDestroy(): void {
    this.subscriptions.forEach(sub => sub.unsubscribe());
  }

  loadNotifications(): void {
    this.isLoading = true;
    this.notificationService.getMyNotifications({ page: 1, pageSize: 50 }).subscribe({
      next: (response) => {
        this.notifications = response.items;
        this.unreadCount = this.notifications.filter(n => !n.isRead).length;
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error cargando notificaciones:', error);
        this.isLoading = false;
      }
    });
  }

  subscribeToSignalR(): void {
    // Escuchar nuevas mascotas
    const newPetSub = this.signalrService.newPetAvailable$.subscribe((petData) => {
      console.log('🐾 Nueva mascota recibida en componente:', petData);
      // Recargar las notificaciones desde el servidor para obtener la persistida
      this.loadNotifications();
    });

    // Escuchar cambios de estado en adopciones
    const statusChangeSub = this.signalrService.adoptionStatusChanged$.subscribe((statusData) => {
      console.log('✅ Cambio de estado recibido en componente:', statusData);
      // Recargar las notificaciones desde el servidor
      this.loadNotifications();
    });

    // Escuchar nuevas solicitudes (admin)
    const newRequestSub = this.signalrService.newAdoptionRequest$.subscribe((adoptionData) => {
      console.log('📋 Nueva solicitud recibida en componente:', adoptionData);
      // Recargar las notificaciones desde el servidor
      this.loadNotifications();
    });

    this.subscriptions.push(newPetSub, statusChangeSub, newRequestSub);
  }

  addNotification(notification: Notification): void {
    this.notifications.unshift(notification);
    this.unreadCount++;
    
    // Mostrar notificación del navegador
    this.showBrowserNotification(notification.title, notification.message);
  }

  showBrowserNotification(title: string, message: string): void {
    if ('Notification' in window && Notification.permission === 'granted') {
      new Notification(title, { body: message, icon: '/assets/logo.png' });
    }
  }

  markAsRead(notification: Notification): void {
    if (notification.isRead || notification.id === 0) return;

    this.notificationService.markAsRead(notification.id).subscribe({
      next: () => {
        notification.isRead = true;
        this.unreadCount--;
      },
      error: (error) => {
        console.error('Error marcando notificación:', error);
      }
    });
  }

  markAllAsRead(): void {
    this.notificationService.markAllAsRead().subscribe({
      next: (response) => {
        this.notifications.forEach(n => n.isRead = true);
        this.unreadCount = 0;
        console.log(`${response.updated} notificaciones marcadas como leídas`);
      },
      error: (error) => {
        console.error('Error marcando todas:', error);
      }
    });
  }

  handleNotificationClick(notification: Notification): void {
    this.markAsRead(notification);

    // Navegar según el tipo de notificación
    if (notification.type === 'NEW_PET' && notification.data?.Pet?.id) {
      this.router.navigate(['/pets', notification.data.Pet.id]);
    } else if (notification.type === 'ADOPTION_STATUS' && notification.data?.AdoptionRequest?.id) {
      this.router.navigate(['/adoption-requests']);
    } else if (notification.type === 'NEW_REQUEST') {
      this.router.navigate(['/adoption-requests']);
    }
  }

  getNotificationIcon(type: string): string {
    switch (type) {
      case 'NEW_PET': return '🐾';
      case 'ADOPTION_STATUS': return '📝';
      case 'NEW_REQUEST': return '📋';
      default: return '🔔';
    }
  }
}
