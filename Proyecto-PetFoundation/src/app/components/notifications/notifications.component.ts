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
  filteredNotifications: Notification[] = [];
  unreadCount = 0;
  isLoading = true;
  activeFilter: 'all' | 'unread' | 'NEW_PET' | 'ADOPTION_STATUS' | 'NEW_REQUEST' = 'all';

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
        this.applyFilter();
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error cargando notificaciones:', error);
        this.isLoading = false;
      }
    });
  }

  subscribeToSignalR(): void {
    const newPetSub = this.signalrService.newPetAvailable$.subscribe(() => {
      this.loadNotifications();
    });

    const statusChangeSub = this.signalrService.adoptionStatusChanged$.subscribe(() => {
      this.loadNotifications();
    });

    const newRequestSub = this.signalrService.newAdoptionRequest$.subscribe(() => {
      this.loadNotifications();
    });

    this.subscriptions.push(newPetSub, statusChangeSub, newRequestSub);
  }

  setFilter(filter: 'all' | 'unread' | 'NEW_PET' | 'ADOPTION_STATUS' | 'NEW_REQUEST'): void {
    this.activeFilter = filter;
    this.applyFilter();
  }

  applyFilter(): void {
    switch (this.activeFilter) {
      case 'all':
        this.filteredNotifications = this.notifications;
        break;
      case 'unread':
        this.filteredNotifications = this.notifications.filter(n => !n.isRead);
        break;
      default:
        this.filteredNotifications = this.notifications.filter(n => n.type === this.activeFilter);
        break;
    }
  }

  trackByNotification(index: number, notification: Notification): number {
    return notification.id;
  }

  markAsRead(notification: Notification): void {
    if (notification.isRead || notification.id === 0) return;

    this.notificationService.markAsRead(notification.id).subscribe({
      next: () => {
        notification.isRead = true;
        this.unreadCount--;
        this.applyFilter();
      },
      error: (error) => {
        console.error('Error marcando notificacion:', error);
      }
    });
  }

  markAllAsRead(): void {
    this.notificationService.markAllAsRead().subscribe({
      next: () => {
        this.notifications.forEach(n => n.isRead = true);
        this.unreadCount = 0;
        this.applyFilter();
      },
      error: (error) => {
        console.error('Error marcando todas:', error);
      }
    });
  }

  handleNotificationClick(notification: Notification): void {
    this.markAsRead(notification);

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

  getTypeLabel(type: string): string {
    switch (type) {
      case 'NEW_PET': return 'Nueva mascota';
      case 'ADOPTION_STATUS': return 'Estado de solicitud';
      case 'NEW_REQUEST': return 'Nueva solicitud';
      default: return 'Notificacion';
    }
  }

  getRelativeTime(date: Date): string {
    const now = new Date();
    const then = new Date(date);
    const diffMs = now.getTime() - then.getTime();
    const diffSec = Math.floor(diffMs / 1000);
    const diffMin = Math.floor(diffSec / 60);
    const diffHour = Math.floor(diffMin / 60);
    const diffDay = Math.floor(diffHour / 24);

    if (diffSec < 60) return 'Justo ahora';
    if (diffMin < 60) return `hace ${diffMin} min`;
    if (diffHour < 24) return `hace ${diffHour}h`;
    if (diffDay < 7) return `hace ${diffDay}d`;
    return then.toLocaleDateString('es-ES', { day: 'numeric', month: 'short' });
  }
}
