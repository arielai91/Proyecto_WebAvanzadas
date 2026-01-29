import { Component, OnInit, OnDestroy, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { trigger, transition, style, animate } from '@angular/animations';
import { AuthService } from '../../services/auth.service';
import { NotificationService } from '../../services/notification.service';
import { SignalrService } from '../../services/signalr.service';
import { ToastService } from '../../services/toast.service';
import { Notification } from '../../models/models';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './navbar.component.html',
  styleUrl: './navbar.component.css',
  animations: [
    trigger('badgeBounce', [
      transition(':enter', [
        style({ transform: 'scale(0)', opacity: 0 }),
        animate('300ms cubic-bezier(0.68, -0.55, 0.265, 1.55)', style({ transform: 'scale(1)', opacity: 1 }))
      ])
    ]),
    trigger('slideDown', [
      transition(':enter', [
        style({ transform: 'translateY(-100%)', opacity: 0 }),
        animate('300ms ease-out', style({ transform: 'translateY(0)', opacity: 1 }))
      ]),
      transition(':leave', [
        animate('200ms ease-in', style({ transform: 'translateY(-100%)', opacity: 0 }))
      ])
    ])
  ]
})
export class NavbarComponent implements OnInit, OnDestroy {
  menuOpen = false;
  isAuthenticated = false;
  isAdmin = false;
  userName = '';
  unreadCount = 0;
  showNotificationBanner = false;
  showNotificationPanel = false;
  previewNotifications: Notification[] = [];
  isLoadingPreview = false;

  private subscriptions: Subscription[] = [];

  constructor(
    public authService: AuthService,
    private notificationService: NotificationService,
    private signalrService: SignalrService,
    private toastService: ToastService,
    private router: Router
  ) {}

  ngOnInit(): void {
    const userSub = this.authService.currentUser$.subscribe(user => {
      this.isAuthenticated = !!user;
      this.isAdmin = user?.roles?.includes('Admin') ?? false;
      this.userName = user?.name || '';

      if (this.isAuthenticated) {
        this.loadUnreadCount();
        this.checkNotificationPermission();
      }
    });

    const petSub = this.signalrService.newPetAvailable$.subscribe(() => {
      if (this.isAuthenticated) {
        setTimeout(() => this.loadUnreadCount(), 500);
      }
    });

    const statusSub = this.signalrService.adoptionStatusChanged$.subscribe(() => {
      if (this.isAuthenticated) {
        setTimeout(() => this.loadUnreadCount(), 500);
      }
    });

    const requestSub = this.signalrService.newAdoptionRequest$.subscribe(() => {
      if (this.isAuthenticated && this.isAdmin) {
        setTimeout(() => this.loadUnreadCount(), 500);
      }
    });

    this.subscriptions.push(userSub, petSub, statusSub, requestSub);
  }

  ngOnDestroy(): void {
    this.subscriptions.forEach(sub => sub.unsubscribe());
  }

  @HostListener('document:keydown.escape')
  onEscapeKey(): void {
    this.closeNotificationPanel();
    this.closeMenu();
  }

  loadUnreadCount(): void {
    this.notificationService.getUnread().subscribe({
      next: (notifications) => {
        this.unreadCount = notifications.length;
      },
      error: (error) => {
        console.error('Error cargando notificaciones:', error);
      }
    });
  }

  toggleMenu(): void {
    this.menuOpen = !this.menuOpen;
    if (this.menuOpen) {
      this.closeNotificationPanel();
    }
  }

  closeMenu(): void {
    this.menuOpen = false;
  }

  toggleNotificationPanel(event: Event): void {
    event.stopPropagation();
    this.showNotificationPanel = !this.showNotificationPanel;
    if (this.showNotificationPanel) {
      this.loadPreviewNotifications();
    }
  }

  closeNotificationPanel(): void {
    this.showNotificationPanel = false;
  }

  loadPreviewNotifications(): void {
    this.isLoadingPreview = true;
    this.notificationService.getMyNotifications({ page: 1, pageSize: 5 }).subscribe({
      next: (response) => {
        this.previewNotifications = response.items;
        this.isLoadingPreview = false;
      },
      error: () => {
        this.isLoadingPreview = false;
      }
    });
  }

  handleDropdownNotificationClick(notification: Notification): void {
    if (!notification.isRead && notification.id !== 0) {
      this.notificationService.markAsRead(notification.id).subscribe({
        next: () => {
          notification.isRead = true;
          this.unreadCount = Math.max(0, this.unreadCount - 1);
        }
      });
    }

    this.closeNotificationPanel();
    this.closeMenu();

    if (notification.type === 'NEW_PET' && notification.data?.Pet?.id) {
      this.router.navigate(['/pets', notification.data.Pet.id]);
    } else if (notification.type === 'ADOPTION_STATUS') {
      this.router.navigate(['/adoption-requests']);
    } else if (notification.type === 'NEW_REQUEST') {
      this.router.navigate(['/adoption-requests']);
    }
  }

  markAllAsReadFromPanel(): void {
    this.notificationService.markAllAsRead().subscribe({
      next: () => {
        this.previewNotifications.forEach(n => n.isRead = true);
        this.unreadCount = 0;
      }
    });
  }

  getNotificationIcon(type: string): string {
    switch (type) {
      case 'NEW_PET': return '🐾';
      case 'ADOPTION_STATUS': return '📝';
      case 'NEW_REQUEST': return '📋';
      default: return '🔔';
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

  getUserInitials(): string {
    if (!this.userName) return '?';
    const parts = this.userName.trim().split(/\s+/);
    if (parts.length >= 2) {
      return (parts[0][0] + parts[1][0]).toUpperCase();
    }
    return this.userName.substring(0, 2).toUpperCase();
  }

  logout(): void {
    this.authService.logout();
    this.signalrService.stopConnection();
    this.router.navigate(['/login']);
    this.menuOpen = false;
    this.showNotificationPanel = false;
  }

  checkNotificationPermission(): void {
    if ('Notification' in window && Notification.permission === 'default') {
      this.showNotificationBanner = true;
    }
  }

  async enableNotifications(): Promise<void> {
    const granted = await this.toastService.requestNotificationPermission();
    if (granted) {
      this.toastService.success('Notificaciones Activadas', 'Recibiras notificaciones de nuevas mascotas y solicitudes');
      this.showNotificationBanner = false;
    } else {
      this.toastService.warning('Notificaciones Bloqueadas', 'Puedes activarlas desde la configuracion del navegador');
      this.showNotificationBanner = false;
    }
  }

  dismissNotificationBanner(): void {
    this.showNotificationBanner = false;
  }
}
