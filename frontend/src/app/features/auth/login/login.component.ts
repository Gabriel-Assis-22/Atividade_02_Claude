import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  template: `
    <div class="auth-page">
      <div class="auth-container">
        <div class="auth-card">
          <div class="auth-brand">
            <span class="auth-icon">🎬</span>
            <h1>Catálogo Tom Hanks</h1>
            <p>Acesse sua conta para continuar</p>
          </div>
          <div *ngIf="erro" class="alert alert-error">{{ erro }}</div>
          <form (ngSubmit)="onLogin()" class="auth-form">
            <div class="form-group">
              <label for="email">E-mail</label>
              <input id="email" type="email" [(ngModel)]="email" name="email" placeholder="seu@email.com" required>
            </div>
            <div class="form-group">
              <div style="display:flex;justify-content:space-between;align-items:center">
                <label for="senha">Senha</label>
                <a routerLink="/auth/forgot-password" style="font-size:.8rem;color:var(--clr-accent)">Esqueceu a senha?</a>
              </div>
              <input id="senha" type="password" [(ngModel)]="senha" name="senha" placeholder="••••••••" required>
            </div>
            <button type="submit" class="btn btn-primary btn-full" [disabled]="loading">
              {{ loading ? 'Entrando...' : 'Entrar' }}
            </button>
          </form>
          <p class="auth-switch">Não tem conta? <a routerLink="/auth/register">Cadastre-se</a></p>
        </div>
      </div>
    </div>
  `,
})
export class LoginComponent {
  private auth = inject(AuthService);
  private router = inject(Router);

  email = '';
  senha = '';
  erro = '';
  loading = false;

  onLogin() {
    this.erro = '';
    this.loading = true;
    this.auth.login(this.email, this.senha).subscribe({
      next: () => this.router.navigate(['/catalog']),
      error: (err) => {
        this.erro = err.error?.erro ?? 'Erro ao fazer login.';
        this.loading = false;
      },
    });
  }
}
