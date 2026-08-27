namespace Application.DTOs.Favorites;

public record AddFavoriteRequest(int TmdbMovieId, string Titulo, string? PosterPath);
public record FavoriteDto(int Id, int TmdbMovieId, string Titulo, string? PosterPath, DateTime CriadoEm);
