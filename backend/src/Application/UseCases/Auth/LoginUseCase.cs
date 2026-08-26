using Application.DTOs.Auth;
using Domain.Repositories;
using Domain.Services;

namespace Application.UseCases.Auth;

public class LoginUseCase(IUsuarioRepository usuarioRepo, IPasswordHasher hasher)
{
    public async Task<Domain.Entities.Usuario?> ExecuteAsync(LoginRequest request)
    {
        var usuario = await usuarioRepo.GetByEmailAsync(request.Email);
        if (usuario is null) return null;
        if (!hasher.Verify(request.Senha, usuario.SenhaHash)) return null;
        return usuario;
    }
}
