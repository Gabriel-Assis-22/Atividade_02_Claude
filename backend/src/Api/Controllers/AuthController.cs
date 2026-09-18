using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Application.DTOs.Auth;
using Infrastructure.ExternalServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(
    IHttpClientFactory httpClientFactory,
    IAuditClient audit,
    ILogger<AuthController> logger) : ControllerBase
{
    private HttpClient AuthClient => httpClientFactory.CreateClient("AuthService");

    private string? ClientIp =>
        Request.Headers["X-Forwarded-For"].FirstOrDefault()
        ?? Request.Headers["X-Real-IP"].FirstOrDefault()
        ?? HttpContext.Connection.RemoteIpAddress?.ToString();

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await ForwardPostAsync("/auth/login", request);
        if (result is ContentResult cr && cr.StatusCode == 200 && cr.Content != null)
        {
            try
            {
                var authResp = JsonSerializer.Deserialize<AuthResponse>(cr.Content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                int? userId = null;
                if (authResp?.Token != null)
                {
                    var handler = new JwtSecurityTokenHandler();
                    if (handler.CanReadToken(authResp.Token))
                    {
                        var jwt = handler.ReadJwtToken(authResp.Token);
                        var idStr = jwt.Claims.FirstOrDefault(c => c.Type == "userId" || c.Type == ClaimTypes.NameIdentifier)?.Value;
                        if (int.TryParse(idStr, out var id)) userId = id;
                    }
                }

                await audit.LogAsync(userId, "login", $"Login realizado por {request.Email}", ClientIp);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Falha ao registrar auditoria de login");
            }
        }
        return result;
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        var rawId = User.FindFirst("userId")?.Value
                 ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                 ?? User.FindFirst("sub")?.Value
                 ?? User.FindFirst("id")?.Value;

        int? userId = int.TryParse(rawId, out var id) ? id : null;

        await audit.LogAsync(userId, "logout", "Usuário encerrou a sessão", ClientIp);
        return Ok(new { mensagem = "Logout efetuado com sucesso." });
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        return await ForwardPostAsync("/auth/register", request);
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        return await ForwardPostAsync("/auth/forgot-password", request);
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        return await ForwardPostAsync("/auth/reset-password", request);
    }

    private async Task<IActionResult> ForwardPostAsync<T>(string relativePath, T payload)
    {
        try
        {
            var content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json");

            var response = await AuthClient.PostAsync(relativePath, content);
            var responseBody = await response.Content.ReadAsStringAsync();

            return Content(responseBody, "application/json", Encoding.UTF8)
                .WithStatusCode((int)response.StatusCode);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro de comunicação com o microsserviço de autenticação em {Path}", relativePath);
            return StatusCode(503, new { erro = "Serviço de autenticação temporariamente indisponível." });
        }
    }
}

internal static class ActionResultsExtensions
{
    public static ContentResult WithStatusCode(this ContentResult result, int statusCode)
    {
        result.StatusCode = statusCode;
        return result;
    }
}
