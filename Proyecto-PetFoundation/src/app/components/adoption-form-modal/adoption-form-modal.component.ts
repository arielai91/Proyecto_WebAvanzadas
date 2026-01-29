import { Component, EventEmitter, Input, Output, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

interface BreedMap {
  [species: string]: string[];
}

@Component({
  selector: 'app-adoption-form-modal',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './adoption-form-modal.component.html',
  styleUrl: './adoption-form-modal.component.css'
})
export class AdoptionFormModalComponent implements OnChanges {
  @Input() isOpen = false;
  @Input() petName = '';
  @Input() petSpecies = '';
  @Input() isLoading = false;

  @Output() confirm = new EventEmitter<string>();
  @Output() cancel = new EventEmitter<void>();

  selectedSpecies = '';
  selectedBreed = '';
  customBreed = '';
  selectedColor = '';
  reason = '';
  submitted = false;

  speciesOptions = ['Perro', 'Gato', 'Ave', 'Conejo', 'Otro'];

  breedsBySpecies: BreedMap = {
    'Perro': ['Labrador', 'Pastor Alemán', 'Bulldog', 'Chihuahua', 'Golden Retriever', 'Husky', 'Mestizo', 'Otra'],
    'Gato': ['Siamés', 'Persa', 'Maine Coon', 'Bengalí', 'Angora', 'Mestizo', 'Otra'],
    'Ave': ['Periquito', 'Canario', 'Cacatúa', 'Loro', 'Otra'],
    'Conejo': ['Holland Lop', 'Rex', 'Mini Lop', 'Angora', 'Otra']
  };

  colorOptions = ['Blanco', 'Negro', 'Marrón', 'Gris', 'Naranja', 'Manchado', 'Sin preferencia'];

  get availableBreeds(): string[] {
    return this.breedsBySpecies[this.selectedSpecies] || [];
  }

  get showCustomBreed(): boolean {
    return this.selectedSpecies === 'Otro' || this.selectedBreed === 'Otra';
  }

  get isReasonValid(): boolean {
    return this.reason.trim().length >= 10;
  }

  get canSubmit(): boolean {
    return this.isReasonValid && !this.isLoading;
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['isOpen'] && changes['isOpen'].currentValue) {
      this.prefillSpecies();
    }
    if (changes['isOpen'] && !changes['isOpen'].currentValue) {
      this.reset();
    }
  }

  onSpeciesChange(): void {
    this.selectedBreed = '';
    this.customBreed = '';
  }

  onSubmit(): void {
    this.submitted = true;
    if (!this.canSubmit) return;

    const parts: string[] = [];

    if (this.selectedSpecies) {
      parts.push(`Especie: ${this.selectedSpecies}`);
    }

    const breed = this.showCustomBreed ? this.customBreed.trim() : this.selectedBreed;
    if (breed) {
      parts.push(`Raza: ${breed}`);
    }

    if (this.selectedColor && this.selectedColor !== 'Sin preferencia') {
      parts.push(`Color: ${this.selectedColor}`);
    }

    let message = '';
    if (parts.length > 0) {
      message = `Preferencias - ${parts.join(' | ')} -- `;
    }
    message += this.reason.trim();

    this.confirm.emit(message);
  }

  onCancel(): void {
    this.cancel.emit();
    this.reset();
  }

  onBackdropClick(event: MouseEvent): void {
    if ((event.target as HTMLElement).classList.contains('adoption-modal-backdrop') && !this.isLoading) {
      this.onCancel();
    }
  }

  private prefillSpecies(): void {
    if (!this.petSpecies) return;
    const normalized = this.petSpecies.trim();
    const match = this.speciesOptions.find(
      s => s.toLowerCase() === normalized.toLowerCase()
    );
    this.selectedSpecies = match || '';
  }

  private reset(): void {
    this.selectedSpecies = '';
    this.selectedBreed = '';
    this.customBreed = '';
    this.selectedColor = '';
    this.reason = '';
    this.submitted = false;
  }
}
