using Application.DTOs.Favorites;
using Application.UseCases.Favorites;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Api.Controllers;

[ApiController]
[Route("api/favorites")]
[Authorize]
public class FavoritesController(
    GetFavoritesUseCase getFavorites,
    AddFavoriteUseCase addFavorite,
    RemoveFavoriteUseCase removeFavorite,
    CheckFavoriteUseCase checkFavorite,
    ILogger<FavoritesController> logger) : ControllerBase
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

    [HttpGet]
    public async Task<IActionResult> GetFavorites()
    {
        if (!CurrentUserId.HasValue)
            return Unauthorized(new { erro = "Sessão expirada. Faça login novamente." });

        try
        {
            return Ok(await getFavorites.ExecuteAsync(CurrentUserId.Value));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao buscar favoritos");
            return StatusCode(500, new { erro = "Erro ao buscar favoritos." });
        }
    }

    [HttpGet("{movieId:int}/check")]
    public async Task<IActionResult> CheckFavorite(int movieId)
    {
        if (!CurrentUserId.HasValue)
            return Unauthorized(new { erro = "Sessão expirada. Faça login novamente." });

        try
        {
            return Ok(new { isFavorito = await checkFavorite.ExecuteAsync(CurrentUserId.Value, movieId) });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao verificar favorito para filme {MovieId}", movieId);
            return StatusCode(500, new { erro = "Erro ao verificar favorito." });
        }
    }

    [HttpPost]
    public async Task<IActionResult> AddFavorite([FromBody] AddFavoriteRequest request)
    {
        if (!CurrentUserId.HasValue)
            return Unauthorized(new { erro = "Sessão expirada. Faça login novamente." });

        try
        {
            await addFavorite.ExecuteAsync(CurrentUserId.Value, request);
            return Ok(new { mensagem = "Favoritado com sucesso." });
        }
        catch (MySqlConnector.MySqlException ex) when (ex.Number == 1452)
        {
            logger.LogWarning(ex, "Usuário ID {UserId} não encontrado no banco ao adicionar favorito", CurrentUserId.Value);
            return Unauthorized(new { erro = "Usuário não encontrado. Faça login novamente." });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao favoritar filme");
            return StatusCode(500, new { erro = "Erro ao favoritar filme." });
        }
    }

    [HttpDelete("{movieId:int}")]
    public async Task<IActionResult> RemoveFavorite(int movieId)
    {
        if (!CurrentUserId.HasValue)
            return Unauthorized(new { erro = "Sessão expirada. Faça login novamente." });

        try
        {
            await removeFavorite.ExecuteAsync(CurrentUserId.Value, movieId);
            return Ok(new { mensagem = "Favorito removido." });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao remover favorito");
            return StatusCode(500, new { erro = "Erro ao remover favorito." });
        }
    }
}
