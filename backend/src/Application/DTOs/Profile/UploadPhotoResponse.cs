namespace Application.DTOs.Profile;

public class UploadPhotoResponse
{
    public string FotoUrl { get; set; } = string.Empty;
    public string Mensagem { get; set; } = "Foto de perfil atualizada com sucesso.";
}
