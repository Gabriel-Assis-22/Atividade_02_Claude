using Application.DTOs.Auth;
using Domain.Repositories;
using Domain.Services;

namespace Application.UseCases.Auth;

public class RegisterUseCase(IUsuarioRepository usuarioRepo, IPasswordHasher hasher)
{
    public async Task<int> ExecuteAsync(RegisterRequest request)
    {
        var hash = hasher.Hash(request.Senha);
        return await usuarioRepo.CreateAsync(request.Nome, request.Email, hash);
    }
}
