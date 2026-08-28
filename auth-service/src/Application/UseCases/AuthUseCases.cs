using Application.DTOs;
using Domain.Entities;
using Domain.Repositories;
using Domain.Services;

namespace Application.UseCases;

public class LoginUseCase(IUsuarioRepository repo, IPasswordHasher hasher)
{
    public async Task<Usuario?> ExecuteAsync(LoginRequest req)
    {
        var usuario = await repo.GetByEmailAsync(req.Email);
        if (usuario is null) return null;
        if (!hasher.Verify(req.Senha, usuario.SenhaHash)) return null;
        return usuario;
    }
}

public class RegisterUseCase(IUsuarioRepository repo, IPasswordHasher hasher)
{
    public async Task<int> ExecuteAsync(RegisterRequest req)
    {
        var role = Enum.TryParse<Role>(req.Role, ignoreCase: true, out var r) ? r : Role.usuario;
        var hash = hasher.Hash(req.Senha);
        return await repo.CreateAsync(req.Nome, req.Email, hash, role);
    }
}

public class ForgotPasswordUseCase(
    IUsuarioRepository usuarioRepo,
    IResetTokenRepository tokenRepo,
    IEmailService emailService)
{
    private readonly string _frontendUrl =
        Environment.GetEnvironmentVariable("FRONTEND_URL") ?? "http://localhost:4200";

    public async Task ExecuteAsync(ForgotPasswordRequest req)
    {
        var usuario = await usuarioRepo.GetByEmailAsync(req.Email);
        // Não revela se o e-mail existe (segurança)
        if (usuario is null) return;

        var token = new ResetToken
        {
            Token     = Guid.NewGuid().ToString("N"), // 32 chars hex, único
            UsuarioId = usuario.Id,
            CriadoEm  = DateTime.UtcNow,
            ExpiraEm  = DateTime.UtcNow.AddMinutes(30),
            Usado     = false,
        };

        await tokenRepo.CreateAsync(token);

        var link = $"{_frontendUrl}/auth/reset-password?token={token.Token}";
        await emailService.SendPasswordResetEmailAsync(usuario.Email, usuario.Nome, link);
    }
}

public class ResetPasswordUseCase(
    IResetTokenRepository tokenRepo,
    IUsuarioRepository usuarioRepo,
    IPasswordHasher hasher)
{
    public async Task<bool> ExecuteAsync(ResetPasswordRequest req)
    {
        var resetToken = await tokenRepo.GetByTokenAsync(req.Token);

        // Validações rigorosas conforme spec
        if (resetToken is null) return false;           // 1. Existe?
        if (!resetToken.IsValido()) return false;       // 2. Não expirou? 3. Não foi usado?

        var novoHash = hasher.Hash(req.NovaSenha);
        await usuarioRepo.UpdatePasswordAsync(resetToken.UsuarioId, novoHash);
        await tokenRepo.MarkAsUsedAsync(req.Token);
        return true;
    }
}
