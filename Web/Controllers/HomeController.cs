using System.Diagnostics;
using System.Security.Claims;
using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Models;

namespace Web.Controllers;

public class HomeController : Controller
{
    private readonly IPostService _postService;
    private readonly ICategoryService _categoryService;
    private readonly ICommentService _commentService;
    private readonly IArticleContentSanitizer _articleContentSanitizer;

    public HomeController(
        IPostService postService,
        ICategoryService categoryService,
        ICommentService commentService,
        IArticleContentSanitizer articleContentSanitizer)
    {
        _postService = postService;
        _categoryService = categoryService;
        _commentService = commentService;
        _articleContentSanitizer = articleContentSanitizer;
    }

    public async Task<IActionResult> Index()
    {
        var model = new HomeIndexViewModel
        {
            Title = "Blog",
            Posts = (await _postService.GetAllAsync())
                .Where(post => post.IsPublished)
                .ToList(),
            Categories = (await _categoryService.GetAllAsync()).ToList()
        };

        return View(model);
    }

    public async Task<IActionResult> Detail(string slug)
    {
        var post = await _postService.GetBySlugAsync(slug);
        if (post is null || !post.IsPublished)
        {
            return NotFound();
        }

        post = post with
        {
            Articles = post.Articles
                .Select(article => article with
                {
                    Content = string.Equals(article.ContentType, "Code", StringComparison.OrdinalIgnoreCase)
                        ? article.Content
                        : _articleContentSanitizer.Sanitize(article.Content)
                })
                .ToList()
        };

        var comments = await _commentService.GetByPostAsync(post.Id);
        var categories = await _categoryService.GetAllAsync();
        var model = new Web.Models.PostDetailViewModel
        {
            Post = post,
            Comments = comments
                .Where(comment => comment.IsApproved || User.IsInRole("Admin"))
                .ToList(),
            Categories = categories
                .Where(category => post.CategoryIds.Contains(category.Id))
                .ToList()
        };

        return View(model);
    }

    public async Task<IActionResult> ByCategory(string slug)
    {
        var categories = (await _categoryService.GetAllAsync()).ToList();
        var category = categories.FirstOrDefault(item => item.Slug == slug);
        if (category is null)
        {
            return NotFound();
        }

        var posts = (await _postService.GetAllAsync())
            .Where(post => post.IsPublished && post.CategoryIds.Contains(category.Id))
            .ToList();

        var model = new HomeIndexViewModel
        {
            Title = $"Blog - {category.Name}",
            Posts = posts,
            Categories = categories
        };

        return View("Index", model);
    }

    public async Task<IActionResult> ByAuthor(string userName)
    {
        var posts = (await _postService.GetAllAsync())
            .Where(post => post.IsPublished && post.AuthorName == userName)
            .ToList();

        var authorName = posts.FirstOrDefault()?.AuthorName ?? "Autor";

        var model = new HomeIndexViewModel
        {
            Title = $"Blog - {authorName}",
            Posts = posts,
            Categories = (await _categoryService.GetAllAsync()).ToList()
        };

        return View("Index", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> AddComment(string slug, string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return BadRequest("El contenido del comentario es requerido.");
        }

        var post = await _postService.GetBySlugAsync(slug);
        if (post is null || !post.IsPublished)
        {
            return NotFound();
        }

        var authorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (authorId is null)
        {
            return Challenge();
        }

        await _commentService.CreateAsync(new CreateCommentDto(
            post.Id,
            authorId,
            content,
            false));

        TempData["CommentSubmitted"] = "Tu comentario se envió y está pendiente de aprobación.";
        return RedirectToAction(nameof(Detail), new { slug });
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
