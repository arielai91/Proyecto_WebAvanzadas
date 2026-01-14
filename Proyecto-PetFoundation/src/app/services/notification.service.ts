import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_CONFIG } from '../config/api.config';
import { 
  Notification, 
  PaginatedNotifications, 
  NotificationFilters 
} from '../models/models';

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  private readonly baseUrl = `${API_CONFIG.baseUrl}/notifications`;

  constructor(private http: HttpClient) {}

  getMyNotifications(filters?: NotificationFilters): Observable<PaginatedNotifications> {
    let params = new HttpParams();
    
    if (filters) {
      if (filters.page) params = params.set('page', filters.page.toString());
      if (filters.pageSize) params = params.set('pageSize', filters.pageSize.toString());
      if (filters.type) params = params.set('type', filters.type);
      if (filters.isRead !== undefined) params = params.set('isRead', filters.isRead.toString());
      if (filters.createdFrom) params = params.set('createdFrom', filters.createdFrom.toISOString());
      if (filters.createdTo) params = params.set('createdTo', filters.createdTo.toISOString());
    }

    return this.http.get<PaginatedNotifications>(this.baseUrl, { params });
  }

  getUnread(): Observable<Notification[]> {
    return this.http.get<Notification[]>(`${this.baseUrl}/unread`);
  }

  markAsRead(id: number): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${id}/read`, {});
  }

  markAllAsRead(): Observable<{ updated: number }> {
    return this.http.post<{ updated: number }>(`${this.baseUrl}/read-all`, {});
  }
}
