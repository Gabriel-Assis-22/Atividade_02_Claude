export interface Movie {
  id: number;
  titulo: string;
  posterUrl: string;
  ano: string;
}

export interface MovieDetail {
  id: number;
  titulo: string;
  sinopse: string;
  posterUrl: string | null;
  posterPath: string;
  ano: string;
  nota: string;
}

export interface Favorite {
  id: number;
  tmdbMovieId: number;
  titulo: string;
  posterPath: string | null;
  criadoEm: string;
}

export interface Comment {
  id: number;
  tmdbMovieId: number;
  texto: string;
  criadoEm: string;
}

export interface AuthResponse {
  token: string;
  nome: string;
  email: string;
}
