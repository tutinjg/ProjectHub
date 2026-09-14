import { HttpClient } from '@angular/common/http';
import { computed, inject, Injectable, signal } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment.development';
import { AuthResponse, LoginRequest, RegisterRequest, UserSession } from '../models/auth.models';

const ACCESS_TOKEN_KEY = 'projecthub_access_token';
const REFRESH_TOKEN_KEY = 'projecthub_refresh_token';
const USER_SESSION_KEY = 'projecthub_user_session';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private http = inject(HttpClient);
  private router = inject(Router);
  private apiUrl = `${environment.apiUrl}/auth`;

  // Estado reactivo con Signals
  private currentUserSignal = signal<UserSession | null>(this.getStoredUser());
  readonly currentUser = this.currentUserSignal.asReadonly();
  readonly isAuthenticated = computed(() => !!this.currentUserSignal());

  login(credentials: LoginRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/login`, credentials).pipe(
      tap(response => this.handleAuthSuccess(response))
    );
  }

  register(data: RegisterRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/register`, data).pipe(
      tap(response => this.handleAuthSuccess(response))
    );
  }

  refreshToken(): Observable<AuthResponse> {
    const refreshToken = this.getRefreshToken();
    const accessToken = this.getAccessToken() ?? '';

    return this.http.post<AuthResponse>(`${this.apiUrl}/refresh-token`, {
      accessToken,
      refreshToken
    }).pipe(
      tap(response => this.handleAuthSuccess(response))
    );
  }

  logout(): void {
    localStorage.removeItem(ACCESS_TOKEN_KEY);
    localStorage.removeItem(REFRESH_TOKEN_KEY);
    localStorage.removeItem(USER_SESSION_KEY);
    this.currentUserSignal.set(null);
    this.router.navigate(['/auth/login']);
  }

  getAccessToken(): string | null {
    return localStorage.getItem(ACCESS_TOKEN_KEY);
  }

  getRefreshToken(): string | null {
    return localStorage.getItem(REFRESH_TOKEN_KEY);
  }

  private handleAuthSuccess(response: AuthResponse): void {
    const session: UserSession = {
      userId: response.userId,
      fullName: response.fullName,
      email: response.email,
      roleName: response.roleName,
      companyId: response.companyId
    };

    localStorage.setItem(ACCESS_TOKEN_KEY, response.accessToken);
    localStorage.setItem(REFRESH_TOKEN_KEY, response.refreshToken);
    localStorage.setItem(USER_SESSION_KEY, JSON.stringify(session));

    this.currentUserSignal.set(session);
  }

  private getStoredUser(): UserSession | null {
    const stored = localStorage.getItem(USER_SESSION_KEY);
    if (!stored) return null;
    try {
      return JSON.parse(stored) as UserSession;
    } catch {
      return null;
    }
  }
}