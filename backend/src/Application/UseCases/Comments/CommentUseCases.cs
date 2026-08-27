using Application.DTOs.Comments;
using Domain.Repositories;

namespace Application.UseCases.Comments;

public class GetCommentsUseCase(IComentarioRepository repo)
{
    public async Task<IEnumerable<CommentDto>> ExecuteAsync(int usuarioId, int tmdbMovieId)
    {
        var comments = await repo.GetByUsuarioAndMovieAsync(usuarioId, tmdbMovieId);
        return comments.Select(c => new CommentDto(c.Id, c.TmdbMovieId, c.Texto, c.CriadoEm));
    }
}

public class AddCommentUseCase(IComentarioRepository repo)
{
    public Task ExecuteAsync(int usuarioId, AddCommentRequest req) =>
        repo.AddAsync(usuarioId, req.TmdbMovieId, req.Texto);
}
