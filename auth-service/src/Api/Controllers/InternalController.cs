using System.Security.Claims;
using Application.DTOs;
using Domain.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

/// <summary>
/// Endpoint interno — chamado pelo backend catálogo para validar JWT e obter dados do usuário.
/// Nunca exposto publicamente (o Nginx não rota /internal/).
/// </summary>
[ApiController]
[Route("internal")]
[Authorize]
public class InternalController(IUsuarioRepository repo) : ControllerBase
{
    // GET /internal/validate
    // O catálogo passa o Bearer token; este endpoint confirma e retorna userId + role
    [HttpGet("validate")]
    public async Task<IActionResult> Validate()
    {
        var userIdStr = User.FindFirst("userId")?.Value;
        if (userIdStr is null) return Unauthorized();

        var usuario = await repo.GetByIdAsync(int.Parse(userIdStr));
        if (usuario is null) return Unauthorized();

        return Ok(new ValidateTokenResponse(
            usuario.Id, usuario.Nome, usuario.Email, usuario.Role.ToString()));
    }
}
