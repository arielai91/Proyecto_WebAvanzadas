import { Component, OnInit, ViewChild, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { PetService } from '../../services/pet.service';
import { AdoptionRequestService } from '../../services/adoption-request.service';
import { AuthService } from '../../services/auth.service';
import { Pet } from '../../models/models';
import { ModalComponent } from '../modal/modal.component';
import * as QRCode from 'qrcode';

@Component({
  selector: 'app-pet-detail',
  standalone: true,
  imports: [CommonModule, ModalComponent],
  templateUrl: './pet-detail.component.html',
  styleUrl: './pet-detail.component.css'
})
export class PetDetailComponent implements OnInit {
  @ViewChild('qrCanvas') qrCanvas!: ElementRef<HTMLCanvasElement>;

  pet: Pet | null = null;
  isLoading = true;
  isAuthenticated = false;
  isAdmin = false;
  isAdopting = false;
  showQrSection = false;

  // Modal state
  showAdoptionModal = false;
  showSuccessModal = false;
  showErrorModal = false;
  errorMessage = '';

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private petService: PetService,
    private adoptionService: AdoptionRequestService,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.isAuthenticated = this.authService.isAuthenticated();
    this.isAdmin = this.authService.isAdmin();
    
    const id = Number(this.route.snapshot.paramMap.get('id'));
    if (id) {
      this.loadPet(id);
    }
  }

  loadPet(id: number): void {
    this.petService.getById(id).subscribe({
      next: (pet) => {
        this.pet = pet;
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error cargando mascota:', error);
        this.isLoading = false;
        this.errorMessage = 'No se pudo cargar la mascota';
        this.showErrorModal = true;
        setTimeout(() => this.router.navigate(['/pets']), 2000);
      }
    });
  }

  openAdoptionModal(): void {
    this.showAdoptionModal = true;
  }

  requestAdoption(reason: string): void {
    if (!this.pet) return;

    this.isAdopting = true;
    this.adoptionService.create({
      petId: this.pet.id,
      message: reason
    }).subscribe({
      next: () => {
        this.isAdopting = false;
        this.showAdoptionModal = false;
        this.showSuccessModal = true;
        
        // Redirigir después de 2 segundos
        setTimeout(() => {
          this.showSuccessModal = false;
          this.router.navigate(['/adoption-requests']);
        }, 2000);
      },
      error: (error) => {
        console.error('Error creando solicitud:', error);
        this.errorMessage = error.error?.error || 'Error al enviar la solicitud';
        this.isAdopting = false;
        this.showAdoptionModal = false;
        this.showErrorModal = true;
      }
    });
  }

  closeAdoptionModal(): void {
    this.showAdoptionModal = false;
  }

  closeSuccessModal(): void {
    this.showSuccessModal = false;
    this.router.navigate(['/adoption-requests']);
  }

  closeErrorModal(): void {
    this.showErrorModal = false;
  }

  editPet(): void {
    if (this.pet) {
      this.router.navigate(['/pets', this.pet.id, 'edit']);
    }
  }

  toggleQrCode(): void {
    this.showQrSection = !this.showQrSection;
    if (this.showQrSection && this.pet) {
      setTimeout(() => this.generateQrCode(), 0);
    }
  }

  private generateQrCode(): void {
    if (!this.pet || !this.qrCanvas) return;
    const value = String(this.pet.id);
    QRCode.toCanvas(this.qrCanvas.nativeElement, value, {
      width: 200,
      margin: 2,
      color: { dark: '#333333', light: '#ffffff' }
    });
  }

  downloadQrCode(): void {
    if (!this.qrCanvas || !this.pet) return;
    const canvas = this.qrCanvas.nativeElement;
    const link = document.createElement('a');
    link.download = `mascota-${this.pet.id}-qr.png`;
    link.href = canvas.toDataURL('image/png');
    link.click();
  }

  goBack(): void {
    this.router.navigate(['/pets']);
  }
}
