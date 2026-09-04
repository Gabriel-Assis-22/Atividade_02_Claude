using Application.DTOs.Comments;
using Application.UseCases.Comments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Api.Controllers;

[ApiController]
[Route("api/comments")]
[Authorize]
public class CommentsController(
    GetCommentsUseCase getComments,
    AddCommentUseCase addComment,
    DeleteCommentUseCase deleteComment,
    ILogger<CommentsController> logger) : ControllerBase
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

    private string CurrentUserRole =>
        User.FindFirst(ClaimTypes.Role)?.Value
        ?? User.FindFirst("role")?.Value
        ?? "usuario";

    [HttpGet("{movieId:int}")]
    public async Task<IActionResult> GetComments(int movieId)
    {
        if (!CurrentUserId.HasValue)
            return Unauthorized(new { erro = "Sessão expirada. Faça login novamente." });

        try
        {
            var result = await getComments.ExecuteAsync(movieId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao buscar comentários para o filme {MovieId}", movieId);
            return StatusCode(500, new { erro = "Erro ao buscar comentários." });
        }
    }

    [HttpPost]
    public async Task<IActionResult> AddComment([FromBody] AddCommentRequest request)
    {
        if (!CurrentUserId.HasValue)
            return Unauthorized(new { erro = "Sessão expirada. Faça login novamente." });

        if (string.IsNullOrWhiteSpace(request.Texto))
            return BadRequest(new { erro = "O comentário não pode estar vazio." });

        try
        {
            await addComment.ExecuteAsync(CurrentUserId.Value, request);
            return Ok(new { mensagem = "Comentário adicionado com sucesso." });
        }
        catch (MySqlConnector.MySqlException ex) when (ex.Number == 1452)
        {
            logger.LogWarning(ex, "Usuário ID {UserId} não encontrado no banco ao adicionar comentário", CurrentUserId.Value);
            return Unauthorized(new { erro = "Usuário não encontrado no banco. Por favor, relogue." });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro inesperado ao adicionar comentário");
            return StatusCode(500, new { erro = "Não foi possível salvar o comentário." });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteComment(int id)
    {
        if (!CurrentUserId.HasValue)
            return Unauthorized(new { erro = "Sessão expirada. Faça login novamente." });

        try
        {
            var result = await deleteComment.ExecuteAsync(id, CurrentUserId.Value, CurrentUserRole);
            return result switch
            {
                DeleteCommentResult.NotFound =>
                    NotFound(new { erro = "Comentário não encontrado." }),
                DeleteCommentResult.Forbidden =>
                    StatusCode(403, new { erro = "Apenas administradores podem apagar comentários de outros usuários." }),
                DeleteCommentResult.Success =>
                    Ok(new { mensagem = "Comentário excluído com sucesso." }),
                _ => StatusCode(500, new { erro = "Erro interno ao excluir comentário." })
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao excluir comentário {CommentId}", id);
            return StatusCode(500, new { erro = "Erro interno ao excluir comentário." });
        }
    }
}
