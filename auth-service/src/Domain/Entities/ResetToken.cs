namespace Domain.Entities;

public class ResetToken
{
    public string Token { get; set; } = string.Empty;
    public int UsuarioId { get; set; }
    public DateTime CriadoEm { get; set; }
    public DateTime ExpiraEm { get; set; }
    public bool Usado { get; set; }

    public bool IsValido() => !Usado && DateTime.UtcNow <= ExpiraEm;
}
