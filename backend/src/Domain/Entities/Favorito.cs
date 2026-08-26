namespace Domain.Entities;

public class Favorito
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public int TmdbMovieId { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string? PosterPath { get; set; }
    public DateTime CriadoEm { get; set; }
}
