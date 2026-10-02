namespace Domain.Entities;

public class Usuario
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = "usuario";
    public string? FotoChave { get; set; }
    public string? FotoUrl { get; set; }
    public string? Bio { get; set; }
    public DateTime CriadoEm { get; set; }
}
