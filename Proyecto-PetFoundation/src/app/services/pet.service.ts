import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_CONFIG } from '../config/api.config';
import { 
  Pet, 
  CreatePetRequest, 
  UpdatePetRequest, 
  PatchPetRequest, 
  PaginatedPets, 
  PetFilters 
} from '../models/models';

@Injectable({
  providedIn: 'root'
})
export class PetService {
  private readonly baseUrl = `${API_CONFIG.baseUrl}/pets`;

  constructor(private http: HttpClient) {}

  getAll(filters?: PetFilters): Observable<PaginatedPets> {
    let params = new HttpParams();
    
    if (filters) {
      if (filters.page) params = params.set('page', filters.page.toString());
      if (filters.pageSize) params = params.set('pageSize', filters.pageSize.toString());
      if (filters.status) params = params.set('status', filters.status);
      if (filters.species) params = params.set('species', filters.species);
      if (filters.size) params = params.set('size', filters.size);
      if (filters.sex) params = params.set('sex', filters.sex);
      if (filters.createdById) params = params.set('createdById', filters.createdById.toString());
      if (filters.minAge) params = params.set('minAge', filters.minAge.toString());
      if (filters.maxAge) params = params.set('maxAge', filters.maxAge.toString());
      if (filters.search) params = params.set('search', filters.search);
      if (filters.createdFrom) params = params.set('createdFrom', filters.createdFrom.toISOString());
      if (filters.createdTo) params = params.set('createdTo', filters.createdTo.toISOString());
    }

    return this.http.get<PaginatedPets>(this.baseUrl, { params });
  }

  getById(id: number): Observable<Pet> {
    return this.http.get<Pet>(`${this.baseUrl}/${id}`);
  }

  create(request: CreatePetRequest): Observable<Pet> {
    return this.http.post<Pet>(this.baseUrl, request);
  }

  update(id: number, request: UpdatePetRequest): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}`, request);
  }

  patch(id: number, request: PatchPetRequest): Observable<void> {
    return this.http.patch<void>(`${this.baseUrl}/${id}`, request);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }

  uploadImage(petId: number, file: File): Observable<{ imageUrl: string; imageId: number }> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<{ imageUrl: string; imageId: number }>(
      `${API_CONFIG.baseUrl}/pets/${petId}/image`,
      formData
    );
  }
}
