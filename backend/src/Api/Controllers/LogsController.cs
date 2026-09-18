using System.Security.Claims;
using System.Text;
using Infrastructure.ExternalServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/logs")]
[Authorize]
public class LogsController(
    IHttpClientFactory httpClientFactory,
    IAuditClient audit,
    ILogger<LogsController> logger) : ControllerBase
{
    private HttpClient LogClient => httpClientFactory.CreateClient("LogService");

    private int? CurrentUserId
    {
        get
        {
            var rawId = User.FindFirst("userId")?.Value
                     ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? User.FindFirst("sub")?.Value
                     ?? User.FindFirst("id")?.Value;

            if (int.TryParse(rawId, out var id)) return id;
            return null;
        }
    }

    private string CurrentUserRole =>
        User.FindFirst(ClaimTypes.Role)?.Value
        ?? User.FindFirst("role")?.Value
        ?? "usuario";

    private string? ClientIp =>
        Request.Headers["X-Forwarded-For"].FirstOrDefault()
        ?? Request.Headers["X-Real-IP"].FirstOrDefault()
        ?? HttpContext.Connection.RemoteIpAddress?.ToString();

    // GET /api/logs?limite=50 (Exclusivo para Administrador)
    [HttpGet]
    public async Task<IActionResult> GetLogs([FromQuery] int limite = 50)
    {
        if (!string.Equals(CurrentUserRole, "admin", StringComparison.OrdinalIgnoreCase))
        {
            logger.LogWarning("Usuário ID {UserId} com papel '{Role}' tentou acessar /api/logs sem permissão.",
                CurrentUserId, CurrentUserRole);

            // Grava evento de tentativa negada por auditoria
            await audit.LogAsync(
                CurrentUserId,
                "tentativa_negada_403",
                "Tentativa não autorizada de consultar logs de auditoria",
                ClientIp);

            return StatusCode(403, new { erro = "Acesso negado. Apenas administradores podem consultar logs de auditoria." });
        }

        try
        {
            var authHeader = Request.Headers.Authorization.ToString();
            using var req = new HttpRequestMessage(HttpMethod.Get, $"/logs?limite={limite}");
            if (!string.IsNullOrWhiteSpace(authHeader))
            {
                req.Headers.TryAddWithoutValidation("Authorization", authHeader);
            }

            var response = await LogClient.SendAsync(req);
            var content = await response.Content.ReadAsStringAsync();

            return Content(content, "application/json", Encoding.UTF8)
                .WithStatusCode((int)response.StatusCode);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro de comunicação ao repassar consulta de logs para o log-service");
            return StatusCode(503, new { erro = "Serviço de logs temporariamente indisponível." });
        }
    }
}
