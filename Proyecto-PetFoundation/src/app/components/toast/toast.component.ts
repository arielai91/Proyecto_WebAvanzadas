import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ToastService, ToastNotification } from '../../services/toast.service';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-toast',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './toast.component.html',
  styleUrl: './toast.component.css'
})
export class ToastComponent implements OnInit, OnDestroy {
  toasts: ToastNotification[] = [];
  private subscription?: Subscription;

  constructor(private toastService: ToastService) {}

  ngOnInit(): void {
    this.subscription = this.toastService.toast$.subscribe(toast => {
      this.toasts.push(toast);

      // Auto-remover despues del duration
      if (toast.duration) {
        setTimeout(() => {
          this.remove(toast.id);
        }, toast.duration);
      }
    });
  }

  ngOnDestroy(): void {
    this.subscription?.unsubscribe();
  }

  remove(id: string): void {
    this.toasts = this.toasts.filter(t => t.id !== id);
  }

  trackByToast(index: number, toast: ToastNotification): string {
    return toast.id;
  }

  getIcon(type: ToastNotification['type']): string {
    switch (type) {
      case 'success': return '✅';
      case 'error': return '❌';
      case 'warning': return '⚠️';
      case 'info': return 'ℹ️';
      default: return 'ℹ️';
    }
  }
}
