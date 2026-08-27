using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Application.DTOs.Movies;

namespace Infrastructure.ExternalServices;

public class TmdbService(HttpClient httpClient)
{
    private const string ImgBase = "https://image.tmdb.org/t/p/w500";
    private int? _tomHanksId;

    private async Task<int> GetTomHanksIdAsync()
    {
        if (_tomHanksId.HasValue) return _tomHanksId.Value;

        var resp = await httpClient.GetFromJsonAsync<TmdbPersonSearch>(
            "/3/search/person?query=Tom+Hanks&language=pt-BR");

        _tomHanksId = resp!.Results[0].Id;
        return _tomHanksId.Value;
    }

    public async Task<IEnumerable<MovieDto>> GetMoviesAsync()
    {
        var personId = await GetTomHanksIdAsync();
        var resp = await httpClient.GetFromJsonAsync<TmdbMovieCredits>(
            $"/3/person/{personId}/movie_credits?language=pt-BR");

        return resp!.Cast
            .Where(f => !string.IsNullOrEmpty(f.PosterPath) && !string.IsNullOrEmpty(f.Title))
            .OrderByDescending(f => f.Popularity)
            .Select(f => new MovieDto(
                f.Id,
                f.Title!,
                $"{ImgBase}{f.PosterPath}",
                f.ReleaseDate?.Length >= 4 ? f.ReleaseDate[..4] : "—"));
    }

    public async Task<MovieDetailDto?> GetMovieDetailAsync(int movieId)
    {
        var f = await httpClient.GetFromJsonAsync<TmdbMovieDetail>(
            $"/3/movie/{movieId}?language=pt-BR");

        if (f is null) return null;

        return new MovieDetailDto(
            f.Id,
            f.Title ?? "",
            string.IsNullOrEmpty(f.Overview) ? "Sinopse não disponível." : f.Overview,
            string.IsNullOrEmpty(f.PosterPath) ? null : $"{ImgBase}{f.PosterPath}",
            f.PosterPath ?? "",
            f.ReleaseDate?.Length >= 4 ? f.ReleaseDate[..4] : "—",
            f.VoteAverage.ToString("F1"));
    }

    // ── TMDB response models (internal, not part of domain) ──
    private record TmdbPersonSearch([property: JsonPropertyName("results")] List<TmdbPerson> Results);
    private record TmdbPerson([property: JsonPropertyName("id")] int Id);

    private record TmdbMovieCredits([property: JsonPropertyName("cast")] List<TmdbCastItem> Cast);
    private record TmdbCastItem(
        [property: JsonPropertyName("id")] int Id,
        [property: JsonPropertyName("title")] string? Title,
        [property: JsonPropertyName("poster_path")] string? PosterPath,
        [property: JsonPropertyName("release_date")] string? ReleaseDate,
        [property: JsonPropertyName("popularity")] double Popularity);

    private record TmdbMovieDetail(
        [property: JsonPropertyName("id")] int Id,
        [property: JsonPropertyName("title")] string? Title,
        [property: JsonPropertyName("overview")] string? Overview,
        [property: JsonPropertyName("poster_path")] string? PosterPath,
        [property: JsonPropertyName("release_date")] string? ReleaseDate,
        [property: JsonPropertyName("vote_average")] double VoteAverage);
}
