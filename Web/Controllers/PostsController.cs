using System.Security.Claims;
using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Web.ViewModels;

namespace Web.Controllers;

[Authorize(Roles = "Admin")]
public class PostsController : Controller
{
    private readonly IPostService _postService;
    private readonly ICategoryService _categoryService;
    private readonly IUserService _userService;

    public PostsController(
        IPostService postService,
        ICategoryService categoryService,
        IUserService userService)
    {
        _postService = postService;
        _categoryService = categoryService;
        _userService = userService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var posts = await _postService.GetAllAsync();

        return View(new PostIndexViewModel { Posts = posts });
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var post = await _postService.GetByIdAsync(id);
        if (post is null)
        {
            return NotFound();
        }

        return View(new PostDetailsViewModel { Post = post });
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        return View(new CreatePostViewModel
        {
            Dto = new CreatePostDto(
                string.Empty,
                string.Empty,
                string.Empty,
                null,
                null,
                false,
                string.Empty,
                new List<ArticleDto> { new(0, string.Empty, "Paragraph", 0, 0) },
                new List<int>()),
            AvailableCategories = await _categoryService.GetAllAsync(),
            AvailableAuthors = await _userService.GetAllAsync()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreatePostViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await PopulateCreateOptionsAsync(model);
            return View(model);
        }

        try
        {
            var post = await _postService.CreateAsync(model.Dto.AuthorId, model.Dto);
            return RedirectToAction(nameof(Details), new { id = post.Id });
        }
        catch (ArgumentException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            await PopulateCreateOptionsAsync(model);
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var post = await _postService.GetByIdAsync(id);
        if (post is null)
        {
            return NotFound();
        }

        return View(new EditPostViewModel
        {
            Id = post.Id,
            Dto = new UpdatePostDto(
                post.Title,
                post.Slug,
                post.Content,
                post.Excerpt,
                post.CoverImage,
                post.IsPublished,
                post.Articles,
                post.CategoryIds),
            AvailableCategories = await _categoryService.GetAllAsync()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditPostViewModel model)
    {
        if (GetCurrentUserId() is null)
        {
            return Challenge();
        }

        if (!ModelState.IsValid)
        {
            model.AvailableCategories = await _categoryService.GetAllAsync();
            return View(model);
        }

        try
        {
            var existingPost = await _postService.GetByIdAsync(model.Id);
            if (existingPost is null)
            {
                return NotFound();
            }

            await _postService.UpdateAsync(existingPost.AuthorId, model.Id, model.Dto);
            return RedirectToAction(nameof(Details), new { id = model.Id });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (ArgumentException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            model.AvailableCategories = await _categoryService.GetAllAsync();
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var post = await _postService.GetByIdAsync(id);
        return post is null ? NotFound() : View(new PostDetailsViewModel { Post = post });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            var existingPost = await _postService.GetByIdAsync(id);
            if (existingPost is null)
            {
                return NotFound();
            }

            await _postService.DeleteAsync(existingPost.AuthorId, id);
            return RedirectToAction(nameof(Index));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    private string? GetCurrentUserId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier);

    private async Task PopulateCreateOptionsAsync(CreatePostViewModel model)
    {
        model.AvailableCategories = await _categoryService.GetAllAsync();
        model.AvailableAuthors = await _userService.GetAllAsync();
    }
}
