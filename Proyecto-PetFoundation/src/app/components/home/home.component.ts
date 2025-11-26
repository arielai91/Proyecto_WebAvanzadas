import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { NavbarComponent } from '../navbar/navbar.component';

interface Pet {
  id: number;
  name: string;
  species: string;
  breed: string;
  age: number;
  size: string;
  gender: string;
  description: string;
  image: string;
}

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, FormsModule, NavbarComponent],
  templateUrl: './home.component.html',
  styleUrl: './home.component.css'
})
export class HomeComponent {
  searchTerm: string = '';
  selectedSpecies: string = 'all';
  selectedSize: string = 'all';

  allPets: Pet[] = [
    { 
      id: 1, 
      name: 'Max', 
      species: 'Perro', 
      breed: 'Labrador', 
      age: 3, 
      size: 'Grande',
      gender: 'Macho',
      description: 'Max es un perro muy amigable y energético. Le encanta jugar y correr.',
      image: 'https://images.unsplash.com/photo-1583511655857-d19b40a7a54e?w=400'
    },
    { 
      id: 2, 
      name: 'Luna', 
      species: 'Gato', 
      breed: 'Siamés', 
      age: 2, 
      size: 'Pequeño',
      gender: 'Hembra',
      description: 'Luna es una gata cariñosa y tranquila. Perfecta para apartamentos.',
      image: 'https://st5.depositphotos.com/4585465/69544/i/450/depositphotos_695440548-stock-photo-cute-kitten-siamese-cat-indoor.jpg'
    },
    { 
      id: 3, 
      name: 'Rocky', 
      species: 'Perro', 
      breed: 'Bulldog', 
      age: 4, 
      size: 'Mediano',
      gender: 'Macho',
      description: 'Rocky es un perro leal y protector. Excelente compañero familiar.',
      image: 'https://images.unsplash.com/photo-1517849845537-4d257902454a?w=400'
    },
    { 
      id: 4, 
      name: 'Bella', 
      species: 'Perro', 
      breed: 'Golden Retriever', 
      age: 2, 
      size: 'Grande',
      gender: 'Hembra',
      description: 'Bella es dulce y gentil. Le encanta estar con niños.',
      image: 'https://images.unsplash.com/photo-1633722715463-d30f4f325e24?w=400'
    },
    { 
      id: 5, 
      name: 'Milo', 
      species: 'Gato', 
      breed: 'Persa', 
      age: 1, 
      size: 'Pequeño',
      gender: 'Macho',
      description: 'Milo es juguetón y curioso. Le encanta explorar.',
      image: 'https://images.unsplash.com/photo-1514888286974-6c03e2ca1dba?w=400'
    },
    { 
      id: 6, 
      name: 'Daisy', 
      species: 'Perro', 
      breed: 'Beagle', 
      age: 3, 
      size: 'Mediano',
      gender: 'Hembra',
      description: 'Daisy es activa y aventurera. Necesita mucho ejercicio.',
      image: 'https://images.unsplash.com/photo-1505628346881-b72b27e84530?w=400'
    }
  ];

  get filteredPets(): Pet[] {
    return this.allPets.filter(pet => {
      const matchesSearch = pet.name.toLowerCase().includes(this.searchTerm.toLowerCase()) ||
                          pet.breed.toLowerCase().includes(this.searchTerm.toLowerCase());
      const matchesSpecies = this.selectedSpecies === 'all' || pet.species === this.selectedSpecies;
      const matchesSize = this.selectedSize === 'all' || pet.size === this.selectedSize;
      
      return matchesSearch && matchesSpecies && matchesSize;
    });
  }

  clearFilters() {
    this.searchTerm = '';
    this.selectedSpecies = 'all';
    this.selectedSize = 'all';
  }
}
