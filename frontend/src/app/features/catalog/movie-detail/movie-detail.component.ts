import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ApiService } from '../../../core/services/api.service';
import { AuthService } from '../../../core/services/auth.service';
import { MovieDetail, Comment } from '../../../shared/models/models';

@Component({
  selector: 'app-movie-detail',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  template: `
    <header class="site-header">
      <div class="header-inner">
        <a routerLink="/catalog" class="logo">🎬 Tom Hanks</a>
        <nav class="header-nav">
          <a routerLink="/catalog">Catálogo</a>
          <a routerLink="/favorites">Meus Favoritos</a>
          <a (click)="auth.logout()" class="btn-logout" style="cursor:pointer">Sair</a>
        </nav>
      </div>
    </header>

    <main class="main-content" *ngIf="filme">
      <div class="movie-detail">
        <div class="movie-detail-poster">
          <img *ngIf="filme.posterUrl" [src]="filme.posterUrl" [alt]="filme.titulo">
          <div *ngIf="!filme.posterUrl" class="poster-placeholder">🎬</div>
        </div>

        <div class="movie-detail-info">
          <a routerLink="/catalog" class="back-link">← Voltar ao catálogo</a>
          <h1>{{ filme.titulo }}</h1>
          <div class="movie-meta">
            <span class="badge">{{ filme.ano }}</span>
            <span class="badge badge-rating">⭐ {{ filme.nota }}</span>
          </div>
          <p class="sinopse">{{ filme.sinopse }}</p>

          <div class="favorite-section">
            <button *ngIf="isFavorito" (click)="onUnfavorite()" class="btn btn-favorited">❤️ Favoritado</button>
            <button *ngIf="!isFavorito" (click)="onFavorite()" class="btn btn-favorite">🤍 Favoritar</button>
          </div>

          <div class="comments-section">
            <h2>Meus Comentários</h2>
            <form (ngSubmit)="onComment()" class="comment-form">
              <textarea [(ngModel)]="novoComentario" name="texto" rows="3" maxlength="1000"
                placeholder="Escreva seu comentário sobre este filme..." required></textarea>
              <button type="submit" class="btn btn-primary">Publicar comentário</button>
            </form>
            <div class="comments-list">
              <p *ngIf="comentarios.length === 0" class="no-comments">Você ainda não comentou este filme.</p>
              <div *ngFor="let c of comentarios" class="comment-card">
                <p class="comment-text">{{ c.texto }}</p>
                <span class="comment-date">{{ c.criadoEm | date:'dd MMM yyyy' }}</span>
              </div>
            </div>
          </div>
        </div>
      </div>
    </main>

    <footer class="site-footer">
      <p>ISW055 · Atividade 02 — Dados fornecidos por <a href="https://www.themoviedb.org" target="_blank">TMDB</a></p>
    </footer>
  `,
})
export class MovieDetailComponent implements OnInit {
  private api = inject(ApiService);
  private route = inject(ActivatedRoute);
  auth = inject(AuthService);

  filme: MovieDetail | null = null;
  isFavorito = false;
  comentarios: Comment[] = [];
  novoComentario = '';

  ngOnInit() {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    this.api.getMovie(id).subscribe(f => { this.filme = f; });
    this.api.checkFavorite(id).subscribe(r => { this.isFavorito = r.isFavorito; });
    this.api.getComments(id).subscribe(c => { this.comentarios = c; });
  }

  onFavorite() {
    if (!this.filme) return;
    this.api.addFavorite({ tmdbMovieId: this.filme.id, titulo: this.filme.titulo, posterPath: this.filme.posterPath }).subscribe(() => {
      this.isFavorito = true;
    });
  }

  onUnfavorite() {
    if (!this.filme) return;
    this.api.removeFavorite(this.filme.id).subscribe(() => { this.isFavorito = false; });
  }

  onComment() {
    if (!this.filme || !this.novoComentario.trim()) return;
    this.api.addComment({ tmdbMovieId: this.filme.id, texto: this.novoComentario.trim() }).subscribe(() => {
      this.api.getComments(this.filme!.id).subscribe(c => { this.comentarios = c; });
      this.novoComentario = '';
    });
  }
}
