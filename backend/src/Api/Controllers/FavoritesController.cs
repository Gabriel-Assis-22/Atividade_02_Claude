using Application.DTOs.Favorites;
using Application.UseCases.Favorites;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/favorites")]
[Authorize]
public class FavoritesController(
    GetFavoritesUseCase getFavorites,
    AddFavoriteUseCase addFavorite,
    RemoveFavoriteUseCase removeFavorite,
    CheckFavoriteUseCase checkFavorite) : ControllerBase
{
    // userId vem SEMPRE do JWT — nunca do body
    private int UserId => int.Parse(User.FindFirst("userId")!.Value);

    [HttpGet]
    public async Task<IActionResult> GetFavorites() =>
        Ok(await getFavorites.ExecuteAsync(UserId));

    [HttpGet("{movieId:int}/check")]
    public async Task<IActionResult> CheckFavorite(int movieId) =>
        Ok(new { isFavorito = await checkFavorite.ExecuteAsync(UserId, movieId) });

    [HttpPost]
    public async Task<IActionResult> AddFavorite([FromBody] AddFavoriteRequest request)
    {
        await addFavorite.ExecuteAsync(UserId, request);
        return Ok(new { mensagem = "Favoritado com sucesso." });
    }

    [HttpDelete("{movieId:int}")]
    public async Task<IActionResult> RemoveFavorite(int movieId)
    {
        await removeFavorite.ExecuteAsync(UserId, movieId);
        return Ok(new { mensagem = "Favorito removido." });
    }
}
