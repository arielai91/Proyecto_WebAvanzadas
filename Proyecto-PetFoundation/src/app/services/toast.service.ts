import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';

export interface ToastNotification {
  id: string;
  type: 'success' | 'error' | 'warning' | 'info';
  title: string;
  message: string;
  duration?: number;
}

@Injectable({
  providedIn: 'root'
})
export class ToastService {
  private toastSubject = new Subject<ToastNotification>();
  public toast$ = this.toastSubject.asObservable();

  private notificationPermissionGranted = false;

  constructor() {
    // Verificar permiso inicial
    if ('Notification' in window) {
      this.notificationPermissionGranted = Notification.permission === 'granted';
    }
  }

  // Solicitar permiso solo cuando el usuario hace click
  requestNotificationPermission(): Promise<boolean> {
    if (!('Notification' in window)) {
      console.warn('Este navegador no soporta notificaciones');
      return Promise.resolve(false);
    }

    if (Notification.permission === 'granted') {
      this.notificationPermissionGranted = true;
      return Promise.resolve(true);
    }

    if (Notification.permission !== 'denied') {
      return Notification.requestPermission().then(permission => {
        this.notificationPermissionGranted = permission === 'granted';
        return this.notificationPermissionGranted;
      });
    }

    return Promise.resolve(false);
  }

  // Mostrar toast (notificación in-app)
  show(type: ToastNotification['type'], title: string, message: string, duration: number = 5000): void {
    const id = Math.random().toString(36).substring(7);
    this.toastSubject.next({ id, type, title, message, duration });
  }

  success(title: string, message: string, duration?: number): void {
    this.show('success', title, message, duration);
  }

  error(title: string, message: string, duration?: number): void {
    this.show('error', title, message, duration);
  }

  warning(title: string, message: string, duration?: number): void {
    this.show('warning', title, message, duration);
  }

  info(title: string, message: string, duration?: number): void {
    this.show('info', title, message, duration);
  }

  // Mostrar notificación del navegador (solo si hay permiso)
  showBrowserNotification(title: string, message: string, icon?: string): void {
    if (this.notificationPermissionGranted && 'Notification' in window) {
      new Notification(title, {
        body: message,
        icon: icon || '/assets/logo.png',
        badge: '/assets/logo.png'
      });
    }
  }
}
