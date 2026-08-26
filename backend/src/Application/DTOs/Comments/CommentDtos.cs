namespace Application.DTOs.Comments;

public record AddCommentRequest(int TmdbMovieId, string Texto);
public record CommentDto(int Id, int TmdbMovieId, string Texto, DateTime CriadoEm);
