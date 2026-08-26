using Infrastructure.ExternalServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/catalog")]
[Authorize]
public class CatalogController(TmdbService tmdb) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetMovies()
    {
        var movies = await tmdb.GetMoviesAsync();
        return Ok(movies);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetMovie(int id)
    {
        var movie = await tmdb.GetMovieDetailAsync(id);
        if (movie is null) return NotFound();
        return Ok(movie);
    }
}
