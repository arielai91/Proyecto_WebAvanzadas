import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AdoptionRequestService } from '../../services/adoption-request.service';
import { AuthService } from '../../services/auth.service';
import { AdoptionRequest, AdoptionRequestFilters, ADOPTION_STATUSES } from '../../models/models';

@Component({
  selector: 'app-adoption-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './adoption-list.component.html',
  styleUrl: './adoption-list.component.css'
})
export class AdoptionListComponent implements OnInit {
  Math = Math;
  requests: AdoptionRequest[] = [];
  totalCount = 0;
  currentPage = 1;
  pageSize = 10;
  isLoading = false;
  isAdmin = false;

  filters: AdoptionRequestFilters = {
    page: 1,
    pageSize: 10
  };

  statuses = Object.values(ADOPTION_STATUSES);

  constructor(
    private adoptionService: AdoptionRequestService,
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.isAdmin = this.authService.isAdmin();
    this.loadRequests();
  }

  loadRequests(): void {
    this.isLoading = true;
    this.filters.page = this.currentPage;
    this.filters.pageSize = this.pageSize;

    this.adoptionService.getAll(this.filters).subscribe({
      next: (response) => {
        this.requests = response.items;
        this.totalCount = response.totalCount;
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error cargando solicitudes:', error);
        this.isLoading = false;
      }
    });
  }

  applyFilters(): void {
    this.currentPage = 1;
    this.loadRequests();
  }

  clearFilters(): void {
    this.filters = {
      page: 1,
      pageSize: 10
    };
    this.currentPage = 1;
    this.loadRequests();
  }

  nextPage(): void {
    if (this.currentPage * this.pageSize < this.totalCount) {
      this.currentPage++;
      this.loadRequests();
    }
  }

  previousPage(): void {
    if (this.currentPage > 1) {
      this.currentPage--;
      this.loadRequests();
    }
  }

  viewPetDetail(petId: number): void {
    this.router.navigate(['/pets', petId]);
  }

  approveRequest(id: number, event: Event): void {
    event.stopPropagation();
    if (confirm('¿Aprobar esta solicitud de adopción?')) {
      this.adoptionService.approve(id).subscribe({
        next: () => {
          this.loadRequests();
        },
        error: (error) => {
          console.error('Error aprobando solicitud:', error);
          alert(error.error?.error || 'Error al aprobar la solicitud');
        }
      });
    }
  }

  rejectRequest(id: number, event: Event): void {
    event.stopPropagation();
    if (confirm('¿Rechazar esta solicitud de adopción?')) {
      this.adoptionService.reject(id).subscribe({
        next: () => {
          this.loadRequests();
        },
        error: (error) => {
          console.error('Error rechazando solicitud:', error);
          alert(error.error?.error || 'Error al rechazar la solicitud');
        }
      });
    }
  }

  cancelRequest(id: number, event: Event): void {
    event.stopPropagation();
    if (confirm('¿Cancelar tu solicitud de adopción?')) {
      this.adoptionService.cancel(id).subscribe({
        next: () => {
          this.loadRequests();
        },
        error: (error) => {
          console.error('Error cancelando solicitud:', error);
          alert(error.error?.error || 'Error al cancelar la solicitud');
        }
      });
    }
  }

  parsePreferences(message: string): { key: string; value: string }[] {
    if (!message) return [];
    const match = message.match(/^Preferencias - (.+?)(\n|$)/);
    if (!match) return [];
    return match[1].split(' | ').map(part => {
      const [key, ...rest] = part.split(': ');
      return { key: key.trim(), value: rest.join(': ').trim() };
    }).filter(p => p.key && p.value);
  }

  parseReason(message: string): string {
    if (!message) return '';
    const parts = message.split('\n\n');
    if (parts.length > 1 && message.startsWith('Preferencias - ')) {
      return parts.slice(1).join('\n\n');
    }
    return message;
  }

  getStatusClass(status: string): string {
    switch (status) {
      case ADOPTION_STATUSES.PENDING: return 'status-pending';
      case ADOPTION_STATUSES.APPROVED: return 'status-approved';
      case ADOPTION_STATUSES.REJECTED: return 'status-rejected';
      case ADOPTION_STATUSES.CANCELLED: return 'status-cancelled';
      default: return '';
    }
  }
}
