using System.Security.Claims;
using Application.DTOs.Profile;
using Application.UseCases.Profile;
using Infrastructure.ExternalServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/profile")]
[Authorize]
public class ProfileController(
    GetProfileUseCase getProfile,
    UpdateProfileUseCase updateProfile,
    UploadProfilePhotoUseCase uploadPhoto,
    Domain.Services.IStorageService storage,
    IAuditClient audit,
    ILogger<ProfileController> logger) : ControllerBase
{
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

    private string? ClientIp =>
        Request.Headers["X-Forwarded-For"].FirstOrDefault()
        ?? Request.Headers["X-Real-IP"].FirstOrDefault()
        ?? HttpContext.Connection.RemoteIpAddress?.ToString();

    /// <summary>
    /// Obtém os dados completos do perfil do usuário logado (incluindo favoritos e foto).
    /// </summary>
    [HttpGet("me")]
    public async Task<IActionResult> GetMyProfile()
    {
        if (!CurrentUserId.HasValue)
            return Unauthorized(new { erro = "Sessão expirada. Faça login novamente." });

        var profile = await getProfile.ExecuteAsync(CurrentUserId.Value);
        if (profile == null)
            return NotFound(new { erro = "Perfil de usuário não encontrado." });

        return Ok(profile);
    }

    /// <summary>
    /// Atualiza as informações do perfil (ex: bio).
    /// Protegido estritamente contra IDOR (Requisito 4): se um TargetUserId alheio for fornecido, retorna 403 Forbidden.
    /// </summary>
    [HttpPut]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        if (!CurrentUserId.HasValue)
            return Unauthorized(new { erro = "Sessão expirada. Faça login novamente." });

        try
        {
            await updateProfile.ExecuteAsync(CurrentUserId.Value, request);

            await audit.LogAsync(
                CurrentUserId.Value,
                "perfil_atualizado",
                "Usuário atualizou as informações do próprio perfil",
                ClientIp);

            return Ok(new { mensagem = "Perfil atualizado com sucesso." });
        }
        catch (UnauthorizedAccessException ex)
        {
            logger.LogWarning("Tentativa de IDOR detectada: Usuário {UserId} tentou alterar perfil do alvo {TargetId}",
                CurrentUserId.Value, request.TargetUserId);

            await audit.LogAsync(
                CurrentUserId.Value,
                "tentativa_idor_bloqueada",
                $"Tentativa negada de alterar perfil do usuário {request.TargetUserId}",
                ClientIp);

            return StatusCode(403, new { erro = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao atualizar perfil do usuário {UserId}", CurrentUserId.Value);
            return StatusCode(500, new { erro = "Erro interno ao atualizar perfil." });
        }
    }

    /// <summary>
    /// Realiza o upload da foto de perfil para o MinIO (Requisito 2).
    /// Valida tipo de arquivo, tamanho máximo e armazena a referência no MariaDB.
    /// </summary>
    [HttpPost("photo")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadPhoto(IFormFile? file)
    {
        if (!CurrentUserId.HasValue)
            return Unauthorized(new { erro = "Sessão expirada. Faça login novamente." });

        if (file == null || file.Length == 0)
            return BadRequest(new { erro = "Nenhum arquivo de imagem foi enviado." });

        try
        {
            using var stream = file.OpenReadStream();
            var result = await uploadPhoto.ExecuteAsync(
                CurrentUserId.Value,
                stream,
                file.FileName,
                file.ContentType,
                file.Length);

            await audit.LogAsync(
                CurrentUserId.Value,
                "foto_perfil_atualizada",
                $"Foto de perfil enviada ao MinIO com sucesso ({file.FileName})",
                ClientIp);

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { erro = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao processar upload da foto de perfil para o usuário {UserId}", CurrentUserId.Value);
            return StatusCode(500, new { erro = "Erro ao processar upload da foto para o Object Storage." });
        }
    }

    /// <summary>
    /// Retorna a foto de perfil armazenada no Garage S3 de forma segura e com cache.
    /// Acesso público para exibição nos avatares.
    /// </summary>
    [AllowAnonymous]
    [HttpGet("photo/{*key}")]
    [ResponseCache(Duration = 86400)]
    public async Task<IActionResult> GetPhoto(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return BadRequest();

        var result = await storage.GetFileAsync(key);
        if (result == null)
            return NotFound();

        return File(result.Value.Stream, result.Value.ContentType);
    }
}
