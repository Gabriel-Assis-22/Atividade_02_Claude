namespace Domain.Entities;

public enum Role { usuario, admin }

public class Usuario
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string SenhaHash { get; set; } = string.Empty;
    public Role Role { get; set; } = Role.usuario;
    public DateTime CriadoEm { get; set; }
}
