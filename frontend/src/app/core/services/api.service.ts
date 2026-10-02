import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { Movie, MovieDetail, Favorite, Comment, AuditLog, UserProfile, UploadPhotoResponse } from '../../shared/models/models';

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
  deleteComment(id: number) {
    return this.http.delete(`${this.base}/comments/${id}`);
  }

  // Audit Logs (Admin)
  getLogs(limite: number = 50) {
    return this.http.get<AuditLog[]>(`${this.base}/logs?limite=${limite}`);
  }

  // Profile
  getMyProfile() {
    return this.http.get<UserProfile>(`${this.base}/profile/me`);
  }

  updateProfile(body: { targetUserId?: number; bio: string | null }) {
    return this.http.put<{ mensagem: string }>(`${this.base}/profile`, body);
  }

  uploadProfilePhoto(file: File) {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<UploadPhotoResponse>(`${this.base}/profile/photo`, formData);
  }
}
