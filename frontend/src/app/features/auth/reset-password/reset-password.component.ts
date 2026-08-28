import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-reset-password',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  template: `
    <div class="auth-page">
      <div class="auth-container">
        <div class="auth-card">
          <div class="auth-brand">
            <span class="auth-icon">🔒</span>
            <h1>Redefinir Senha</h1>
            <p>Digite sua nova senha abaixo</p>
          </div>

          <div *ngIf="mensagem()" class="alert" style="background:rgba(76,175,130,.15);border:1px solid var(--clr-success);color:var(--clr-success)">
            {{ mensagem() }}
          </div>
          <div *ngIf="erro()" class="alert alert-error">{{ erro() }}</div>

          <form *ngIf="token() && !sucesso()" (ngSubmit)="onSubmit()" class="auth-form">
            <div class="form-group">
              <label for="senha">Nova Senha <small>(mínimo 6 caracteres)</small></label>
              <input id="senha" type="password" [(ngModel)]="novaSenha" name="senha" placeholder="••••••••" minlength="6" required>
            </div>
            <div class="form-group">
              <label for="confirmarSenha">Confirmar Nova Senha</label>
              <input id="confirmarSenha" type="password" [(ngModel)]="confirmarSenha" name="confirmarSenha" placeholder="••••••••" minlength="6" required>
            </div>
            <button type="submit" class="btn btn-primary btn-full" [disabled]="loading()">
              {{ loading() ? 'Redefinindo...' : 'Atualizar Senha' }}
            </button>
          </form>

          <div *ngIf="!token()" class="alert alert-error">
            Token de redefinição não encontrado ou inválido. Solicite um novo link de recuperação.
          </div>

          <div *ngIf="sucesso()" style="margin-top:1.5rem;text-align:center">
            <a routerLink="/auth/login" class="btn btn-primary btn-full">Ir para o Login</a>
          </div>

          <p class="auth-switch" *ngIf="!sucesso()">
            Voltar para o <a routerLink="/auth/login">Login</a>
          </p>
        </div>
      </div>
    </div>
  `,
})
export class ResetPasswordComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private auth = inject(AuthService);

  token = signal('');
  novaSenha = '';
  confirmarSenha = '';
  erro = signal('');
  mensagem = signal('');
  loading = signal(false);
  sucesso = signal(false);

  ngOnInit() {
    const t = this.route.snapshot.queryParamMap.get('token') ?? '';
    this.token.set(t);
    if (!t) {
      this.erro.set('Link de redefinição inválido ou incompleto.');
    }
  }

  onSubmit() {
    this.erro.set('');
    this.mensagem.set('');

    if (!this.token()) {
      this.erro.set('Token inválido ou ausente.');
      return;
    }

    if (this.novaSenha.length < 6) {
      this.erro.set('A senha deve ter pelo menos 6 caracteres.');
      return;
    }

    if (this.novaSenha !== this.confirmarSenha) {
      this.erro.set('As senhas não conferem.');
      return;
    }

    this.loading.set(true);
    this.auth.resetPassword(this.token(), this.novaSenha).subscribe({
      next: (res) => {
        this.mensagem.set(res?.mensagem ?? 'Senha redefinida com sucesso!');
        this.sucesso.set(true);
        this.loading.set(false);
      },
      error: (err) => {
        this.erro.set(err.error?.erro ?? 'Não foi possível redefinir a senha. O link pode ter expirado.');
        this.loading.set(false);
      },
    });
  }
}
