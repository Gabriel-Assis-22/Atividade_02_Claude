import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ApiService } from '../../../core/services/api.service';
import { AuthService } from '../../../core/services/auth.service';
import { Favorite } from '../../../shared/models/models';

const TMDB_IMG = 'https://image.tmdb.org/t/p/w500';

@Component({
  selector: 'app-favorites-page',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <header class="site-header">
      <div class="header-inner">
        <a routerLink="/catalog" class="logo">🎬 Tom Hanks</a>
        <nav class="header-nav">
          <a routerLink="/catalog">Catálogo</a>
          <a routerLink="/favorites" class="active">Meus Favoritos</a>
          <a (click)="auth.logout()" class="btn-logout" style="cursor:pointer">Sair</a>
        </nav>
      </div>
    </header>

    <main class="main-content">
      <div class="catalog-header">
        <h1>❤️ Meus <span class="highlight">Favoritos</span></h1>
        <p>{{ favoritos().length }} filme{{ favoritos().length !== 1 ? 's' : '' }} salvo{{ favoritos().length !== 1 ? 's' : '' }}</p>
      </div>

      <div *ngIf="favoritos().length === 0 && !loading()" class="empty-state">
        <span class="empty-icon">🎬</span>
        <p>Você ainda não favoritou nenhum filme.</p>
        <a routerLink="/catalog" class="btn btn-primary">Explorar catálogo</a>
      </div>

      <div *ngIf="!loading() && favoritos().length > 0" class="movies-grid">
        <a *ngFor="let fav of favoritos()" [routerLink]="['/movie', fav.tmdbMovieId]" class="movie-card">
          <div class="movie-poster">
            <img *ngIf="fav.posterPath" [src]="getImgUrl(fav.posterPath)" [alt]="fav.titulo" loading="lazy">
            <div *ngIf="!fav.posterPath" class="poster-placeholder">🎬</div>
          </div>
          <div class="movie-info">
            <h2 class="movie-title">{{ fav.titulo }}</h2>
            <span class="badge-heart">❤️ Favoritado</span>
          </div>
        </a>
      </div>
    </main>

    <footer class="site-footer">
      <p>ISW055 · Atividade 02 — Dados fornecidos por <a href="https://www.themoviedb.org" target="_blank">TMDB</a></p>
    </footer>
  `,
})
export class FavoritesPageComponent implements OnInit {
  private api = inject(ApiService);
  auth = inject(AuthService);

  favoritos = signal<Favorite[]>([]);
  loading = signal(true);

  ngOnInit() {
    this.api.getFavorites().subscribe({
      next: (f) => {
        this.favoritos.set(f);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
      }
    });
  }

  getImgUrl(path: string) { return `${TMDB_IMG}${path}`; }
}
