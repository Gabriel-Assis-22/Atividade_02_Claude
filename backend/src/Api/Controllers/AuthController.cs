using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Application.DTOs.Auth;
using Application.UseCases.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(LoginUseCase loginUseCase, RegisterUseCase registerUseCase) : ControllerBase
{
    private static readonly string JwtSecret =
        Environment.GetEnvironmentVariable("JWT_SECRET") ?? "dev_secret";

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Senha))
            return BadRequest(new { erro = "Preencha todos os campos." });

        var usuario = await loginUseCase.ExecuteAsync(request);
        if (usuario is null)
            return Unauthorized(new { erro = "E-mail ou senha inválidos." });

        var token = GenerateToken(usuario.Id, usuario.Nome, usuario.Email);
        return Ok(new AuthResponse(token, usuario.Nome, usuario.Email));
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Nome) ||
            string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Senha))
            return BadRequest(new { erro = "Preencha todos os campos." });

        if (request.Senha.Length < 6)
            return BadRequest(new { erro = "A senha deve ter pelo menos 6 caracteres." });

        try
        {
            await registerUseCase.ExecuteAsync(request);
            return Ok(new { mensagem = "Cadastro realizado com sucesso." });
        }
        catch (MySqlConnector.MySqlException ex) when (ex.Number == 1062)
        {
            return Conflict(new { erro = "Este e-mail já está cadastrado." });
        }
    }

    private static string GenerateToken(int userId, string nome, string email)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtSecret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim("userId", userId.ToString()),
            new Claim(ClaimTypes.Name, nome),
            new Claim(ClaimTypes.Email, email),
        };

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddDays(1),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
