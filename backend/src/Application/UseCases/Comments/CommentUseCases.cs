using Application.DTOs.Comments;
using Domain.Repositories;

namespace Application.UseCases.Comments;

public class GetCommentsUseCase(IComentarioRepository repo)
{
    public async Task<IEnumerable<CommentDto>> ExecuteAsync(int tmdbMovieId)
    {
        var comments = await repo.GetByMovieAsync(tmdbMovieId);
        return comments.Select(c => new CommentDto(c.Id, c.UsuarioId, c.UsuarioNome, c.TmdbMovieId, c.Texto, c.CriadoEm));
    }
}

public class AddCommentUseCase(IComentarioRepository repo)
{
    public Task ExecuteAsync(int usuarioId, AddCommentRequest req) =>
        repo.AddAsync(usuarioId, req.TmdbMovieId, req.Texto);
}

public enum DeleteCommentResult
{
    Success,
    NotFound,
    Forbidden
}

public class DeleteCommentUseCase(IComentarioRepository repo)
{
    public async Task<DeleteCommentResult> ExecuteAsync(int comentarioId, int currentUserId, string currentUserRole)
    {
        var comentario = await repo.GetByIdAsync(comentarioId);
        if (comentario is null)
        {
            return DeleteCommentResult.NotFound;
        }

        bool isOwner = comentario.UsuarioId == currentUserId;
        bool isAdmin = string.Equals(currentUserRole, "admin", StringComparison.OrdinalIgnoreCase);

        if (!isOwner && !isAdmin)
        {
            return DeleteCommentResult.Forbidden;
        }

        await repo.DeleteAsync(comentarioId);
        return DeleteCommentResult.Success;
    }
}
