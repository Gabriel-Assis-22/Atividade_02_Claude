import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  template: `
    <div class="auth-page">
      <div class="auth-container">
        <div class="auth-card">
          <div class="auth-brand">
            <span class="auth-icon">🎬</span>
            <h1>Criar Conta</h1>
            <p>Junte-se ao catálogo de filmes</p>
          </div>
          <div *ngIf="erro" class="alert alert-error">{{ erro }}</div>
          <form (ngSubmit)="onRegister()" class="auth-form">
            <div class="form-group">
              <label for="nome">Nome</label>
              <input id="nome" type="text" [(ngModel)]="nome" name="nome" placeholder="Seu nome completo" required>
            </div>
            <div class="form-group">
              <label for="email">E-mail</label>
              <input id="email" type="email" [(ngModel)]="email" name="email" placeholder="seu@email.com" required>
            </div>
            <div class="form-group">
              <label for="senha">Senha <small>(mínimo 6 caracteres)</small></label>
              <input id="senha" type="password" [(ngModel)]="senha" name="senha" placeholder="••••••••" minlength="6" required>
            </div>
            <button type="submit" class="btn btn-primary btn-full" [disabled]="loading">
              {{ loading ? 'Cadastrando...' : 'Cadastrar' }}
            </button>
          </form>
          <p class="auth-switch">Já tem conta? <a routerLink="/auth/login">Fazer login</a></p>
        </div>
      </div>
    </div>
  `,
})
export class RegisterComponent {
  private auth = inject(AuthService);
  private router = inject(Router);

  nome = ''; email = ''; senha = '';
  erro = ''; loading = false;

  onRegister() {
    this.erro = '';
    if (this.senha.length < 6) { this.erro = 'A senha deve ter pelo menos 6 caracteres.'; return; }
    this.loading = true;
    this.auth.register(this.nome, this.email, this.senha).subscribe({
      next: () => this.router.navigate(['/auth/login']),
      error: (err) => { this.erro = err.error?.erro ?? 'Erro ao cadastrar.'; this.loading = false; },
    });
  }
}
