using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Application.DTOs;
using Application.UseCases;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Api.Controllers;

[ApiController]
[Route("auth")]
public class AuthController(
    LoginUseCase login,
    RegisterUseCase register,
    ForgotPasswordUseCase forgotPassword,
    ResetPasswordUseCase resetPassword) : ControllerBase
{
    private static readonly string JwtSecret =
        Environment.GetEnvironmentVariable("JWT_SECRET") ?? "dev_secret";

    // POST /auth/login
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Senha))
            return BadRequest(new { erro = "Preencha todos os campos." });

        var usuario = await login.ExecuteAsync(req);
        if (usuario is null)
            return Unauthorized(new { erro = "E-mail ou senha inválidos." });

        var token = GenerateToken(usuario.Id, usuario.Nome, usuario.Email, usuario.Role.ToString());
        return Ok(new LoginResponse(token, usuario.Nome, usuario.Email, usuario.Role.ToString()));
    }

    // POST /auth/register
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Nome) ||
            string.IsNullOrWhiteSpace(req.Email) ||
            string.IsNullOrWhiteSpace(req.Senha))
            return BadRequest(new { erro = "Preencha todos os campos." });

        if (req.Senha.Length < 6)
            return BadRequest(new { erro = "A senha deve ter pelo menos 6 caracteres." });

        try
        {
            await register.ExecuteAsync(req);
            return Ok(new { mensagem = "Cadastro realizado com sucesso." });
        }
        catch (MySqlConnector.MySqlException ex) when (ex.Number == 1062)
        {
            return Conflict(new { erro = "Este e-mail já está cadastrado." });
        }
    }

    // POST /auth/forgot-password
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Email))
            return BadRequest(new { erro = "Informe o e-mail." });

        await forgotPassword.ExecuteAsync(req);
        // Sempre retorna 200 para não revelar se o e-mail existe
        return Ok(new { mensagem = "Se este e-mail estiver cadastrado, você receberá um link em breve." });
    }

    // POST /auth/reset-password
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Token) || string.IsNullOrWhiteSpace(req.NovaSenha))
            return BadRequest(new { erro = "Token e nova senha são obrigatórios." });

        if (req.NovaSenha.Length < 6)
            return BadRequest(new { erro = "A senha deve ter pelo menos 6 caracteres." });

        var sucesso = await resetPassword.ExecuteAsync(req);
        if (!sucesso)
            return BadRequest(new { erro = "Token inválido, expirado ou já utilizado." });

        return Ok(new { mensagem = "Senha redefinida com sucesso. Faça login com a nova senha." });
    }

    // ── JWT com claim role ────────────────────────────────────────────────────
    private static string GenerateToken(int userId, string nome, string email, string role)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtSecret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim("userId", userId.ToString()),
            new Claim(ClaimTypes.Name, nome),
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Role, role),
        };

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddDays(1),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
