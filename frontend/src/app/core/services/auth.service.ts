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

  login(email: string, senha: string) {
    return this.http.post<AuthResponse>(`${environment.apiUrl}/auth/login`, { email, senha }).pipe(
      tap(res => localStorage.setItem(this.TOKEN_KEY, res.token))
    );
  }

  register(nome: string, email: string, senha: string) {
    return this.http.post(`${environment.apiUrl}/auth/register`, { nome, email, senha });
  }

  logout() {
    localStorage.removeItem(this.TOKEN_KEY);
    this.router.navigate(['/auth/login']);
  }

  getToken(): string | null {
    return localStorage.getItem(this.TOKEN_KEY);
  }

  isLoggedIn(): boolean {
    return !!this.getToken();
  }
}
