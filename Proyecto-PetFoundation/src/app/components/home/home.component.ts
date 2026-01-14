import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { PetService } from '../../services/pet.service';
import { AuthService } from '../../services/auth.service';
import { SignalrService } from '../../services/signalr.service';
import { Pet } from '../../models/models';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './home.component.html',
  styleUrl: './home.component.css'
})
export class HomeComponent implements OnInit, OnDestroy {
  featuredPets: Pet[] = [];
  isLoading = true;
  isAuthenticated = false;
  newPetNotification: string = '';
  showNotification: boolean = false;
  
  private subscriptions: Subscription[] = [];

  constructor(
    private petService: PetService,
    private authService: AuthService,
    private signalrService: SignalrService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.isAuthenticated = this.authService.isAuthenticated();
    this.loadFeaturedPets();
    this.subscribeToSignalR();
  }

  ngOnDestroy(): void {
    this.subscriptions.forEach(sub => sub.unsubscribe());
  }

  loadFeaturedPets(): void {
    this.petService.getAll({ page: 1, pageSize: 6, status: 'Available' }).subscribe({
      next: (response) => {
        this.featuredPets = response.items;
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error cargando mascotas:', error);
        this.isLoading = false;
      }
    });
  }

  subscribeToSignalR(): void {
    const newPetSub = this.signalrService.newPetAvailable$.subscribe((petData) => {
      console.log('🐾 Nueva mascota recibida:', petData);
      this.showNewPetNotification(petData.Pet?.name || 'Nueva mascota');
      this.loadFeaturedPets();
    });

    const statusSub = this.signalrService.adoptionStatusChanged$.subscribe((statusData) => {
      console.log('✅ Estado de adopción actualizado:', statusData);
      this.loadFeaturedPets();
    });

    this.subscriptions.push(newPetSub, statusSub);
  }

  showNewPetNotification(petName: string): void {
    this.newPetNotification = `¡Nueva mascota disponible: ${petName}!`;
    this.showNotification = true;
    
    setTimeout(() => {
      this.showNotification = false;
    }, 5000);
  }

  viewAllPets(): void {
    this.router.navigate(['/pets']);
  }

  viewPetDetail(petId: number): void {
    this.router.navigate(['/pets', petId]);
  }
}
