using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Infrastructure.ExternalServices;

public interface IAuditClient
{
    Task LogAsync(int? usuarioId, string acao, string? detalhes = null, string? ipOrigem = null);
}

public class AuditClient(HttpClient httpClient, ILogger<AuditClient> logger) : IAuditClient
{
    public async Task LogAsync(int? usuarioId, string acao, string? detalhes = null, string? ipOrigem = null)
    {
        try
        {
            var payload = new
            {
                usuarioId,
                acao,
                detalhes,
                ipOrigem,
                timestamp = DateTime.UtcNow
            };

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
            var response = await httpClient.PostAsync("/logs", content, cts.Token);
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Log-service retornou status {StatusCode} ao registrar evento {Acao}",
                    response.StatusCode, acao);
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Não foi possível enviar o evento de auditoria '{Acao}' para o log-service", acao);
        }
    }
}
