namespace Application.DTOs.Profile;

public class UpdateProfileRequest
{
    // Enviado opcionalmente pelo cliente. Se enviado e for diferente do usuário logado, 
    // o backend rejeita com 403 Forbidden para comprovação de proteção contra IDOR (Requisito 4)
    public int? TargetUserId { get; set; }
    public string? Bio { get; set; }
}
