using Application.DTOs.Comments;
using Application.UseCases.Comments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/comments")]
[Authorize]
public class CommentsController(GetCommentsUseCase getComments, AddCommentUseCase addComment) : ControllerBase
{
    // userId vem SEMPRE do JWT — nunca do body
    private int UserId => int.Parse(User.FindFirst("userId")!.Value);

    [HttpGet("{movieId:int}")]
    public async Task<IActionResult> GetComments(int movieId) =>
        Ok(await getComments.ExecuteAsync(UserId, movieId));

    [HttpPost]
    public async Task<IActionResult> AddComment([FromBody] AddCommentRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Texto))
            return BadRequest(new { erro = "O comentário não pode estar vazio." });

        await addComment.ExecuteAsync(UserId, request);
        return Ok(new { mensagem = "Comentário adicionado." });
    }
}
