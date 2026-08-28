import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-forgot-password',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  template: `
    <div class="auth-page">
      <div class="auth-container">
        <div class="auth-card">
          <div class="auth-brand">
            <span class="auth-icon">🔑</span>
            <h1>Recuperar Senha</h1>
            <p>Informe seu e-mail para receber as instruções</p>
          </div>

          <div *ngIf="mensagem()" class="alert" style="background:rgba(76,175,130,.15);border:1px solid var(--clr-success);color:var(--clr-success)">
            {{ mensagem() }}
          </div>
          <div *ngIf="erro()" class="alert alert-error">{{ erro() }}</div>

          <form *ngIf="!sucesso()" (ngSubmit)="onSubmit()" class="auth-form">
            <div class="form-group">
              <label for="email">E-mail cadastrado</label>
              <input id="email" type="email" [(ngModel)]="email" name="email" placeholder="seu@email.com" required>
            </div>
            <button type="submit" class="btn btn-primary btn-full" [disabled]="loading()">
              {{ loading() ? 'Enviando instruções...' : 'Enviar link de recuperação' }}
            </button>
          </form>

          <p class="auth-switch">Lembrou a senha? <a routerLink="/auth/login">Voltar ao login</a></p>
        </div>
      </div>
    </div>
  `,
})
export class ForgotPasswordComponent {
  private auth = inject(AuthService);

  email = '';
  erro = signal('');
  mensagem = signal('');
  loading = signal(false);
  sucesso = signal(false);

  onSubmit() {
    if (!this.email.trim()) {
      this.erro.set('Por favor, informe seu e-mail.');
      return;
    }

    this.erro.set('');
    this.mensagem.set('');
    this.loading.set(true);

    this.auth.forgotPassword(this.email.trim()).subscribe({
      next: (res) => {
        this.mensagem.set(res?.mensagem ?? 'Instruções enviadas! Verifique seu e-mail.');
        this.sucesso.set(true);
        this.loading.set(false);
      },
      error: (err) => {
        this.erro.set(err.error?.erro ?? 'Erro ao solicitar recuperação de senha.');
        this.loading.set(false);
      },
    });
  }
}
