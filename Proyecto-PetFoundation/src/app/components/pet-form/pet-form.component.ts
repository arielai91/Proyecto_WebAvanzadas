import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { PetService } from '../../services/pet.service';
import { CreatePetRequest, UpdatePetRequest, PET_STATUSES, PET_SIZES, PET_SEXES } from '../../models/models';

@Component({
  selector: 'app-pet-form',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './pet-form.component.html',
  styleUrl: './pet-form.component.css'
})
export class PetFormComponent implements OnInit {
  isEditMode = false;
  petId: number | null = null;
  isLoading = false;
  isSaving = false;
  selectedFile: File | null = null;
  imagePreview: string | null = null;

  petData: any = {
    name: '',
    species: '',
    breed: '',
    age: undefined,
    sex: 'Macho',
    size: 'Mediano',
    color: '',
    description: '',
    status: 'Available'
  };

  statuses = Object.values(PET_STATUSES);
  sizes = Object.values(PET_SIZES);
  sexes = Object.values(PET_SEXES);

  errorMessage = '';

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private petService: PetService
  ) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.isEditMode = true;
      this.petId = Number(id);
      this.loadPet(this.petId);
    }
  }

  loadPet(id: number): void {
    this.isLoading = true;
    this.petService.getById(id).subscribe({
      next: (pet) => {
        this.petData = {
          name: pet.name,
          species: pet.species,
          breed: pet.breed || '',
          age: pet.age,
          sex: pet.sex,
          size: pet.size,
          color: pet.color || '',
          description: pet.description || '',
          status: pet.status
        };
        this.imagePreview = pet.coverImageUrl || null;
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error cargando mascota:', error);
        this.errorMessage = 'Error al cargar la mascota';
        this.isLoading = false;
      }
    });
  }

  onFileSelected(event: any): void {
    const file = event.target.files[0];
    if (file) {
      this.selectedFile = file;
      
      // Preview
      const reader = new FileReader();
      reader.onload = (e: any) => {
        this.imagePreview = e.target.result;
      };
      reader.readAsDataURL(file);
    }
  }

  onSubmit(): void {
    if (!this.petData.name || !this.petData.species || !this.petData.sex || !this.petData.size || !this.petData.description || this.petData.description.length < 10) {
      this.errorMessage = 'Por favor completa todos los campos obligatorios. La descripción debe tener al menos 10 caracteres.';
      return;
    }

    this.isSaving = true;
    this.errorMessage = '';

    if (this.isEditMode && this.petId) {
      this.updatePet();
    } else {
      this.createPet();
    }
  }

  createPet(): void {
    this.petService.create(this.petData as CreatePetRequest).subscribe({
      next: (pet) => {
        console.log('Mascota creada:', pet);
        
        // Si hay imagen, intentar subirla (pero no es obligatoria)
        if (this.selectedFile) {
          this.uploadImage(pet.id);
        } else {
          // Mascota creada exitosamente sin imagen
          this.isSaving = false;
          this.router.navigate(['/pets', pet.id]);
        }
      },
      error: (error) => {
        console.error('Error creando mascota:', error);
        this.errorMessage = error.error?.errors 
          ? this.formatValidationErrors(error.error.errors)
          : error.error?.error || error.error?.title || 'Error al crear la mascota';
        this.isSaving = false;
      }
    });
  }

  updatePet(): void {
    if (!this.petId) return;

    this.petService.update(this.petId, this.petData as UpdatePetRequest).subscribe({
      next: () => {
        console.log('Mascota actualizada');
        
        // Si hay nueva imagen, subirla
        if (this.selectedFile) {
          this.uploadImage(this.petId!);
        } else {
          this.router.navigate(['/pets', this.petId]);
        }
      },
      error: (error) => {
        console.error('Error actualizando mascota:', error);
        this.errorMessage = error.error?.error || 'Error al actualizar la mascota';
        this.isSaving = false;
      }
    });
  }

  uploadImage(petId: number): void {
    if (!this.selectedFile) return;

    this.petService.uploadImage(petId, this.selectedFile).subscribe({
      next: (response) => {
        console.log('Imagen subida exitosamente:', response);
        this.isSaving = false;
        this.router.navigate(['/pets', petId]);
      },
      error: (error) => {
        console.error('Error subiendo imagen:', error);
        // La mascota ya fue creada, solo falló la imagen
        // Mostramos advertencia pero navegamos igual
        alert('⚠️ La mascota fue creada pero no se pudo subir la imagen. Puedes editarla después para agregar una foto.');
        this.isSaving = false;
        this.router.navigate(['/pets', petId]);
      }
    });
  }

  cancel(): void {
    if (this.isEditMode && this.petId) {
      this.router.navigate(['/pets', this.petId]);
    } else {
      this.router.navigate(['/pets']);
    }
  }

  formatValidationErrors(errors: any): string {
    const messages: string[] = [];
    for (const key in errors) {
      if (errors.hasOwnProperty(key)) {
        const errorArray = errors[key];
        if (Array.isArray(errorArray)) {
          messages.push(...errorArray);
        }
      }
    }
    return messages.join(' ');
  }
}
