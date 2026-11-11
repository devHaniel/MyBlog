using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.ViewModels;

namespace Web.Controllers;

[Authorize(Roles = "Admin")]
public class CommentsController : Controller
{
    private readonly ICommentService _commentService;
    private readonly IPostService _postService;

    public CommentsController(ICommentService commentService, IPostService postService)
    {
        _commentService = commentService;
        _postService = postService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int postId)
    {
        if (postId <= 0)
        {
            return BadRequest();
        }

        var comments = await _commentService.GetByPostAsync(postId);

        return View(new CommentIndexViewModel
        {
            PostId = postId,
            Comments = comments
        });
    }

    [HttpGet]
    public async Task<IActionResult> Create(int postId)
    {
        var post = await _postService.GetByIdAsync(postId);
        return post is null ? NotFound() : View(new CreateCommentViewModel { PostId = postId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateCommentViewModel model)
    {
        var authorId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (authorId is null)
        {
            return Challenge();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            await _commentService.CreateAsync(model.Dto with
            {
                AuthorId = authorId,
                PostId = model.PostId,
                IsApproved = model.Dto.IsApproved
            });
            return RedirectToAction(nameof(Index), new { postId = model.PostId });
        }
        catch (ArgumentException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var comment = await _commentService.GetByIdAsync(id);
        return comment is null
            ? NotFound()
            : View(new EditCommentViewModel
            {
                Id = comment.Id,
                PostId = comment.PostId,
                Dto = new UpdateCommentDto(comment.Content, comment.IsApproved)
            });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditCommentViewModel model)
    {
        var comment = await _commentService.GetByIdAsync(model.Id);
        if (comment is null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            model.PostId = comment.PostId;
            return View(model);
        }

        try
        {
            await _commentService.UpdateAsync(model.Id, model.Dto);
            return RedirectToAction(nameof(Index), new { postId = comment.PostId });
        }
        catch (ArgumentException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            model.PostId = comment.PostId;
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var comment = await _commentService.GetByIdAsync(id);
        return comment is null
            ? NotFound()
            : View(new CommentDetailsViewModel { Comment = comment });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var comment = await _commentService.GetByIdAsync(id);
        if (comment is null)
        {
            return NotFound();
        }

        await _commentService.DeleteAsync(id);
        return RedirectToAction(nameof(Index), new { postId = comment.PostId });
    }
}
