import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_CONFIG } from '../config/api.config';
import { 
  AdoptionRequest, 
  CreateAdoptionRequestRequest, 
  PaginatedAdoptionRequests, 
  AdoptionRequestFilters 
} from '../models/models';

@Injectable({
  providedIn: 'root'
})
export class AdoptionRequestService {
  private readonly baseUrl = `${API_CONFIG.baseUrl}/adoptionrequests`;

  constructor(private http: HttpClient) {}

  getAll(filters?: AdoptionRequestFilters): Observable<PaginatedAdoptionRequests> {
    let params = new HttpParams();
    
    if (filters) {
      if (filters.page) params = params.set('page', filters.page.toString());
      if (filters.pageSize) params = params.set('pageSize', filters.pageSize.toString());
      if (filters.status) params = params.set('status', filters.status);
      if (filters.petId) params = params.set('petId', filters.petId.toString());
      if (filters.userId) params = params.set('userId', filters.userId.toString());
      if (filters.decisionById) params = params.set('decisionById', filters.decisionById.toString());
      if (filters.createdFrom) params = params.set('createdFrom', filters.createdFrom.toISOString());
      if (filters.createdTo) params = params.set('createdTo', filters.createdTo.toISOString());
    }

    return this.http.get<PaginatedAdoptionRequests>(this.baseUrl, { params });
  }

  getById(id: number): Observable<AdoptionRequest> {
    return this.http.get<AdoptionRequest>(`${this.baseUrl}/${id}`);
  }

  create(request: CreateAdoptionRequestRequest): Observable<AdoptionRequest> {
    return this.http.post<AdoptionRequest>(this.baseUrl, request);
  }

  approve(id: number): Observable<AdoptionRequest> {
    return this.http.post<AdoptionRequest>(`${this.baseUrl}/${id}/approve`, {});
  }

  reject(id: number): Observable<AdoptionRequest> {
    return this.http.post<AdoptionRequest>(`${this.baseUrl}/${id}/reject`, {});
  }

  cancel(id: number): Observable<AdoptionRequest> {
    return this.http.post<AdoptionRequest>(`${this.baseUrl}/${id}/cancel`, {});
  }
}
