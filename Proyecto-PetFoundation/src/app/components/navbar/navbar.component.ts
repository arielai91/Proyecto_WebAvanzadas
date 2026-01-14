import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { NotificationService } from '../../services/notification.service';
import { SignalrService } from '../../services/signalr.service';
import { ToastService } from '../../services/toast.service';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './navbar.component.html',
  styleUrl: './navbar.component.css'
})
export class NavbarComponent implements OnInit, OnDestroy {
  menuOpen = false;
  isAuthenticated = false;
  isAdmin = false;
  userName = '';
  unreadCount = 0;
  showNotificationBanner = false;
  
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

    // Escuchar notificaciones en tiempo real y actualizar contador
    const petSub = this.signalrService.newPetAvailable$.subscribe(() => {
      if (this.isAuthenticated) {
        console.log('🐾 Nueva mascota detectada en navbar, actualizando contador');
        setTimeout(() => this.loadUnreadCount(), 500); // Pequeño delay para que el backend persista la notificación
      }
    });

    const statusSub = this.signalrService.adoptionStatusChanged$.subscribe(() => {
      if (this.isAuthenticated) {
        console.log('✅ Estado cambiado detectado en navbar, actualizando contador');
        setTimeout(() => this.loadUnreadCount(), 500);
      }
    });

    const requestSub = this.signalrService.newAdoptionRequest$.subscribe(() => {
      if (this.isAuthenticated && this.isAdmin) {
        console.log('📋 Nueva solicitud detectada en navbar, actualizando contador');
        setTimeout(() => this.loadUnreadCount(), 500);
      }
    });

    this.subscriptions.push(userSub, petSub, statusSub, requestSub);
  }

  ngOnDestroy(): void {
    this.subscriptions.forEach(sub => sub.unsubscribe());
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
  }

  logout(): void {
    this.authService.logout();
    this.signalrService.stopConnection();
    this.router.navigate(['/login']);
    this.menuOpen = false;
  }

  checkNotificationPermission(): void {
    if ('Notification' in window && Notification.permission === 'default') {
      this.showNotificationBanner = true;
    }
  }

  async enableNotifications(): Promise<void> {
    const granted = await this.toastService.requestNotificationPermission();
    if (granted) {
      this.toastService.success('✅ Notificaciones Activadas', 'Recibirás notificaciones de nuevas mascotas y solicitudes');
      this.showNotificationBanner = false;
    } else {
      this.toastService.warning('⚠️ Notificaciones Bloqueadas', 'Puedes activarlas desde la configuración del navegador');
      this.showNotificationBanner = false;
    }
  }

  dismissNotificationBanner(): void {
    this.showNotificationBanner = false;
  }
}
