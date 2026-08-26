import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { Movie, MovieDetail, Favorite, Comment } from '../../shared/models/models';

@Injectable({ providedIn: 'root' })
export class ApiService {
  private http = inject(HttpClient);
  private base = environment.apiUrl;

  // Catalog
  getMovies() { return this.http.get<Movie[]>(`${this.base}/catalog`); }
  getMovie(id: number) { return this.http.get<MovieDetail>(`${this.base}/catalog/${id}`); }

  // Favorites
  getFavorites() { return this.http.get<Favorite[]>(`${this.base}/favorites`); }
  checkFavorite(movieId: number) { return this.http.get<{ isFavorito: boolean }>(`${this.base}/favorites/${movieId}/check`); }
  addFavorite(body: { tmdbMovieId: number; titulo: string; posterPath: string }) {
    return this.http.post(`${this.base}/favorites`, body);
  }
  removeFavorite(movieId: number) { return this.http.delete(`${this.base}/favorites/${movieId}`); }

  // Comments
  getComments(movieId: number) { return this.http.get<Comment[]>(`${this.base}/comments/${movieId}`); }
  addComment(body: { tmdbMovieId: number; texto: string }) {
    return this.http.post(`${this.base}/comments`, body);
  }
}
