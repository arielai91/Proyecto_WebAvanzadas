import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { PetService } from '../../services/pet.service';
import { AuthService } from '../../services/auth.service';
import { Pet, PetFilters, PET_STATUSES, PET_SIZES, PET_SEXES } from '../../models/models';

@Component({
  selector: 'app-pet-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './pet-list.component.html',
  styleUrl: './pet-list.component.css'
})
export class PetListComponent implements OnInit {
  Math = Math;
  pets: Pet[] = [];
  totalCount = 0;
  currentPage = 1;
  pageSize = 12;
  isLoading = false;
  isAdmin = false;

  filters: PetFilters = {
    page: 1,
    pageSize: 12
  };

  statuses = Object.values(PET_STATUSES);
  sizes = Object.values(PET_SIZES);
  sexes = Object.values(PET_SEXES);

  constructor(
    private petService: PetService,
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.isAdmin = this.authService.isAdmin();
    this.loadPets();
  }

  loadPets(): void {
    this.isLoading = true;
    this.filters.page = this.currentPage;
    this.filters.pageSize = this.pageSize;

    this.petService.getAll(this.filters).subscribe({
      next: (response) => {
        this.pets = response.items;
        this.totalCount = response.totalCount;
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error cargando mascotas:', error);
        this.isLoading = false;
      }
    });
  }

  applyFilters(): void {
    this.currentPage = 1;
    this.loadPets();
  }

  clearFilters(): void {
    this.filters = {
      page: 1,
      pageSize: 12
    };
    this.currentPage = 1;
    this.loadPets();
  }

  nextPage(): void {
    if (this.currentPage * this.pageSize < this.totalCount) {
      this.currentPage++;
      this.loadPets();
    }
  }

  previousPage(): void {
    if (this.currentPage > 1) {
      this.currentPage--;
      this.loadPets();
    }
  }

  viewPetDetail(petId: number): void {
    this.router.navigate(['/pets', petId]);
  }

  createPet(): void {
    this.router.navigate(['/pets/create']);
  }

  editPet(petId: number, event: Event): void {
    event.stopPropagation();
    this.router.navigate(['/pets', petId, 'edit']);
  }

  deletePet(petId: number, event: Event): void {
    event.stopPropagation();
    if (confirm('¿Estás seguro de que deseas eliminar esta mascota?')) {
      this.petService.delete(petId).subscribe({
        next: () => {
          this.loadPets();
        },
        error: (error) => {
          console.error('Error eliminando mascota:', error);
          alert('Error al eliminar la mascota');
        }
      });
    }
  }

  getStatusClass(status: string): string {
    switch (status) {
      case PET_STATUSES.AVAILABLE: return 'status-available';
      case PET_STATUSES.PENDING: return 'status-pending';
      case PET_STATUSES.ADOPTED: return 'status-adopted';
      default: return '';
    }
  }
}
