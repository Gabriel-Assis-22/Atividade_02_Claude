using System.Text;
using System.Text.Json;
using Application.DTOs.Auth;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IHttpClientFactory httpClientFactory, ILogger<AuthController> logger) : ControllerBase
{
    private HttpClient AuthClient => httpClientFactory.CreateClient("AuthService");

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        return await ForwardPostAsync("/auth/login", request);
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
