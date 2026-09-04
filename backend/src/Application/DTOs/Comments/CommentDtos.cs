namespace Application.DTOs.Comments;

public record AddCommentRequest(int TmdbMovieId, string Texto);
public record CommentDto(int Id, int UsuarioId, string UsuarioNome, int TmdbMovieId, string Texto, DateTime CriadoEm);
