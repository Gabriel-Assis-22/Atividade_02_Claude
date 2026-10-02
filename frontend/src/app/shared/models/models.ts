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
  usuarioId: number;
  usuarioNome: string;
  tmdbMovieId: number;
  texto: string;
  criadoEm: string;
}

export interface AuthResponse {
  token: string;
  nome: string;
  email: string;
  role?: string;
}

export interface AuditLog {
  id: string;
  usuarioId: number | null;
  acao: string;
  detalhes: string | null;
  ipOrigem: string | null;
  timestamp: string;
}

export interface UserProfile {
  id: number;
  nome: string;
  email: string;
  role: string;
  fotoUrl: string | null;
  bio: string | null;
  criadoEm: string;
  favoritos: Favorite[];
}

export interface UploadPhotoResponse {
  fotoUrl: string;
  mensagem: string;
}
