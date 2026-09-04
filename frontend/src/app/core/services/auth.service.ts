import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { tap } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { AuthResponse } from '../../shared/models/models';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private http = inject(HttpClient);
  private router = inject(Router);
  private readonly TOKEN_KEY = 'jwt_token';
  private readonly USER_KEY = 'auth_user';

  login(email: string, senha: string) {
    return this.http.post<AuthResponse>(`${environment.apiUrl}/auth/login`, { email, senha }).pipe(
      tap(res => {
        localStorage.setItem(this.TOKEN_KEY, res.token);
        localStorage.setItem(this.USER_KEY, JSON.stringify(res));
      })
    );
  }

  register(nome: string, email: string, senha: string) {
    return this.http.post(`${environment.apiUrl}/auth/register`, { nome, email, senha });
  }

  forgotPassword(email: string) {
    return this.http.post<{ mensagem: string }>(`${environment.apiUrl}/auth/forgot-password`, { email });
  }

  resetPassword(token: string, novaSenha: string) {
    return this.http.post<{ mensagem: string }>(`${environment.apiUrl}/auth/reset-password`, { token, novaSenha });
  }

  logout() {
    localStorage.removeItem(this.TOKEN_KEY);
    localStorage.removeItem(this.USER_KEY);
    this.router.navigate(['/auth/login']);
  }

  getToken(): string | null {
    return localStorage.getItem(this.TOKEN_KEY);
  }

  isLoggedIn(): boolean {
    return !!this.getToken();
  }

  getCurrentUser(): AuthResponse | null {
    const raw = localStorage.getItem(this.USER_KEY);
    if (!raw) return null;
    try {
      return JSON.parse(raw);
    } catch {
      return null;
    }
  }

  getCurrentRole(): string {
    const user = this.getCurrentUser();
    if (user?.role) return user.role;
    const token = this.getToken();
    if (!token) return 'usuario';
    try {
      const parts = token.split('.');
      if (parts.length === 3) {
        const payload = JSON.parse(atob(parts[1]));
        return payload['role'] || payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || 'usuario';
      }
    } catch { }
    return 'usuario';
  }

  getCurrentUserId(): number | null {
    const token = this.getToken();
    if (!token) return null;
    try {
      const parts = token.split('.');
      if (parts.length === 3) {
        const payload = JSON.parse(atob(parts[1]));
        const id = payload['userId'] || payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'] || payload['sub'];
        return id ? Number(id) : null;
      }
    } catch { }
    return null;
  }

  isAdmin(): boolean {
    return this.getCurrentRole().toLowerCase() === 'admin';
  }
}
