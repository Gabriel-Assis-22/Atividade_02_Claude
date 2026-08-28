using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Domain.Services;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Email;

public class MailtrapEmailService : IEmailService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<MailtrapEmailService> _logger;
    private readonly string _apiToken;

    public MailtrapEmailService(HttpClient httpClient, ILogger<MailtrapEmailService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _apiToken = Environment.GetEnvironmentVariable("MAILTRAP_API_TOKEN") ?? string.Empty;
    }

    public async Task SendPasswordResetEmailAsync(string toEmail, string toName, string resetLink)
    {
        if (string.IsNullOrWhiteSpace(_apiToken))
        {
            _logger.LogWarning("MAILTRAP_API_TOKEN não configurada. E-mail não enviado.");
            return;
        }

        try
        {
            var payload = new
            {
                from = new { email = "hello@demomailtrap.co", name = "Catálogo Tom Hanks" },
                to = new[] { new { email = toEmail, name = toName } },
                subject = "Recuperação de Senha — Catálogo Tom Hanks",
                category = "Password Reset",
                html = $@"
                    <div style=""font-family:Inter,sans-serif;max-width:520px;margin:0 auto;background:#161920;color:#e8eaf0;padding:2rem;border-radius:12px"">
                      <h1 style=""color:#e8a838;font-size:1.4rem;margin-bottom:1rem"">🎬 Recuperação de Senha</h1>
                      <p>Olá, <strong>{toName}</strong>!</p>
                      <p style=""margin:1rem 0"">Recebemos uma solicitação para redefinir sua senha. Clique no botão abaixo para criar uma nova senha. <strong>O link expira em 30 minutos.</strong></p>
                      <a href=""{resetLink}""
                         style=""display:inline-block;background:#e8a838;color:#0d0f14;padding:.75rem 2rem;border-radius:8px;text-decoration:none;font-weight:700;margin:1rem 0"">
                        Redefinir minha senha
                      </a>
                      <p style=""font-size:.8rem;color:#8892a4;margin-top:2rem"">
                        Se você não solicitou a recuperação, ignore este e-mail. Sua senha permanecerá a mesma.
                      </p>
                      <hr style=""border-color:#2a2f42;margin:1.5rem 0"">
                      <p style=""font-size:.75rem;color:#8892a4"">ISW055 — Infraestrutura e Aplicações em Cloud · 2026</p>
                    </div>",
                text = $"Redefinir senha: {resetLink} (expira em 30 minutos)"
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, "https://send.api.mailtrap.io/api/send")
            {
                Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiToken);

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Mailtrap API retornou status {StatusCode}: {Error}", response.StatusCode, errorBody);
            }
            else
            {
                _logger.LogInformation("E-mail de recuperação enviado com sucesso para {Email}", toEmail);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar e-mail de recuperação para {Email}", toEmail);
        }
    }
}
