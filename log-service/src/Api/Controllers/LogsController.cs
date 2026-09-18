using System.Security.Claims;
using Api.DTOs;
using Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("logs")]
public class LogsController(IRedisLogService redisLogService, ILogger<LogsController> logger) : ControllerBase
{
    // POST /logs (Chamado internamente pelos microsserviços)
    [HttpPost]
    public async Task<IActionResult> CreateLog([FromBody] CreateLogEventRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Acao))
        {
            return BadRequest(new { erro = "A ação do evento é obrigatória." });
        }

        try
        {
            var ip = request.IpOrigem ?? HttpContext.Connection.RemoteIpAddress?.ToString();
            var reqWithIp = request with { IpOrigem = ip };

            var id = await redisLogService.AddEventAsync(reqWithIp);
            return Ok(new { id, mensagem = "Evento de auditoria registrado com sucesso." });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao gravar evento de auditoria no Redis");
            return StatusCode(500, new { erro = "Falha ao gravar evento de auditoria." });
        }
    }

    // GET /logs?limite=50 (Exclusivo para administradores)
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetLogs([FromQuery] int limite = 50)
    {
        var role = User.FindFirst(ClaimTypes.Role)?.Value
                ?? User.FindFirst("role")?.Value
                ?? "usuario";

        if (!string.Equals(role, "admin", StringComparison.OrdinalIgnoreCase))
        {
            logger.LogWarning("Tentativa não autorizada de consulta a logs por usuário não-admin. Role: {Role}", role);
            return StatusCode(403, new { erro = "Acesso negado. Apenas administradores podem consultar logs de auditoria." });
        }

        try
        {
            var events = await redisLogService.GetEventsAsync(limite);
            return Ok(events);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao consultar logs de auditoria no Redis");
            return StatusCode(500, new { erro = "Falha ao consultar logs de auditoria." });
        }
    }
}
