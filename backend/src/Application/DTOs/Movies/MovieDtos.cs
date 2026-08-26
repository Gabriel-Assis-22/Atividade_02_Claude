namespace Application.DTOs.Movies;

public record MovieDto(int Id, string Titulo, string PosterUrl, string Ano);
public record MovieDetailDto(int Id, string Titulo, string Sinopse, string? PosterUrl, string PosterPath, string Ano, string Nota);
