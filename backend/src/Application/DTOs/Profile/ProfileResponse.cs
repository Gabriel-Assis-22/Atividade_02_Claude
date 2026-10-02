using Application.DTOs.Favorites;

namespace Application.DTOs.Profile;

public class ProfileResponse
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = "usuario";
    public string? FotoUrl { get; set; }
    public string? Bio { get; set; }
    public DateTime CriadoEm { get; set; }
    public IEnumerable<FavoriteDto> Favoritos { get; set; } = [];
}
