import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ApiService } from '../../../core/services/api.service';
import { AuthService } from '../../../core/services/auth.service';
import { Movie } from '../../../shared/models/models';

@Component({
  selector: 'app-catalog-page',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <header class="site-header">
      <div class="header-inner">
        <a routerLink="/catalog" class="logo">🎬 Tom Hanks</a>
        <nav class="header-nav">
          <a routerLink="/catalog" class="active">Catálogo</a>
          <a routerLink="/favorites">Meus Favoritos</a>
          <a (click)="auth.logout()" class="btn-logout" style="cursor:pointer">Sair</a>
        </nav>
      </div>
    </header>

    <main class="main-content">
      <div class="catalog-header">
        <h1>Filmografia de <span class="highlight">Tom Hanks</span></h1>
        <p>{{ filmes().length }} filmes encontrados</p>
      </div>

      <div *ngIf="erro()" class="alert alert-error">{{ erro() }}</div>
      <div *ngIf="loading()" class="empty-state"><span class="empty-icon">⏳</span><p>Carregando catálogo...</p></div>

      <div *ngIf="!loading()" class="movies-grid">
        <a *ngFor="let filme of filmes()" [routerLink]="['/movie', filme.id]" class="movie-card">
          <div class="movie-poster">
            <img [src]="filme.posterUrl" [alt]="filme.titulo" loading="lazy">
          </div>
          <div class="movie-info">
            <h2 class="movie-title">{{ filme.titulo }}</h2>
            <span class="movie-year">{{ filme.ano }}</span>
          </div>
        </a>
      </div>
    </main>

    <footer class="site-footer">
      <p>ISW055 · Atividade 02 — Dados fornecidos por <a href="https://www.themoviedb.org" target="_blank">TMDB</a></p>
    </footer>
  `,
})
export class CatalogPageComponent implements OnInit {
  private api = inject(ApiService);
  auth = inject(AuthService);

  filmes = signal<Movie[]>([]);
  loading = signal(true);
  erro = signal('');

  ngOnInit() {
    this.api.getMovies().subscribe({
      next: (data) => {
        this.filmes.set(data);
        this.loading.set(false);
      },
      error: () => {
        this.erro.set('Não foi possível carregar o catálogo.');
        this.loading.set(false);
      },
    });
  }
}
