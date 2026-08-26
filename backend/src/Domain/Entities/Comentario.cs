namespace Domain.Entities;

public class Comentario
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public int TmdbMovieId { get; set; }
    public string Texto { get; set; } = string.Empty;
    public DateTime CriadoEm { get; set; }
}
