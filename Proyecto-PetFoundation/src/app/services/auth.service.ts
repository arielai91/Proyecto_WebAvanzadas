import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { API_CONFIG } from '../config/api.config';
import { LoginRequest, RegisterRequest, AuthResponse, UserProfile } from '../models/models';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly TOKEN_KEY = 'auth_token';
  private readonly USER_KEY = 'user_data';
  
  private currentUserSubject = new BehaviorSubject<UserProfile | null>(this.getUserFromStorage());
  public currentUser$ = this.currentUserSubject.asObservable();

  constructor(private http: HttpClient) {}

  register(request: RegisterRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${API_CONFIG.baseUrl}/auth/register`, request)
      .pipe(tap(response => this.handleAuthResponse(response)));
  }

  login(request: LoginRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${API_CONFIG.baseUrl}/auth/login`, request)
      .pipe(tap(response => this.handleAuthResponse(response)));
  }

  getMe(): Observable<UserProfile> {
    return this.http.get<UserProfile>(`${API_CONFIG.baseUrl}/auth/me`);
  }

  logout(): void {
    localStorage.removeItem(this.TOKEN_KEY);
    localStorage.removeItem(this.USER_KEY);
    this.currentUserSubject.next(null);
  }

  getToken(): string | null {
    return localStorage.getItem(this.TOKEN_KEY);
  }

  isAuthenticated(): boolean {
    return !!this.getToken();
  }

  isAdmin(): boolean {
    const user = this.currentUserSubject.value;
    return user?.roles?.includes('Admin') ?? false;
  }

  getCurrentUser(): UserProfile | null {
    return this.currentUserSubject.value;
  }

  private handleAuthResponse(response: AuthResponse): void {
    localStorage.setItem(this.TOKEN_KEY, response.token);
    const user: UserProfile = {
      userId: response.userId.toString(),
      email: response.email,
      name: response.name,
      roles: response.roles
    };
    localStorage.setItem(this.USER_KEY, JSON.stringify(user));
    this.currentUserSubject.next(user);
  }

  private getUserFromStorage(): UserProfile | null {
    const userData = localStorage.getItem(this.USER_KEY);
    return userData ? JSON.parse(userData) : null;
  }
}
