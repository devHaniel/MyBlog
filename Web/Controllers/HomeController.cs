using System.Diagnostics;
using System.Security.Claims;
using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Markdig;
using Web.Models;

namespace Web.Controllers;

public class HomeController : Controller
{
    private readonly IPostService _postService;
    private readonly ICategoryService _categoryService;
    private readonly ICommentService _commentService;
    private readonly IArticleContentSanitizer _articleContentSanitizer;
    private readonly IUserService _userService;

    public HomeController(
        IPostService postService,
        ICategoryService categoryService,
        ICommentService commentService,
        IArticleContentSanitizer articleContentSanitizer,
        IUserService userService)
    {
        _postService = postService;
        _categoryService = categoryService;
        _commentService = commentService;
        _articleContentSanitizer = articleContentSanitizer;
        _userService = userService;
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

    [HttpGet("/posts/{slug}", Name = "post-detail")]
    [HttpGet("/Home/Detail", Name = "legacy-post-detail")]
    public async Task<IActionResult> Detail(string slug)
    {
        var post = await _postService.GetBySlugAsync(slug);
        if (post is null || !post.IsPublished)
        {
            return NotFound();
        }

        post = post with
        {
            Content = string.IsNullOrWhiteSpace(post.Content)
                ? post.Content
                : _articleContentSanitizer.Sanitize(Markdown.ToHtml(post.Content)),
            Articles = post.Articles
                .Select(article => article with
                {
                    Content = string.Equals(article.ContentType, "Code", StringComparison.OrdinalIgnoreCase)
                        ? article.Content
                        : _articleContentSanitizer.Sanitize(Markdown.ToHtml(article.Content))
                })
                .ToList()
        };

        var comments = (await _commentService.GetByPostAsync(post.Id))
            .Where(comment => comment.IsApproved || User.IsInRole("Admin"))
            .ToList();
        var categories = await _categoryService.GetAllAsync();
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var currentUser = currentUserId is null
            ? null
            : await _userService.GetByIdAsync(currentUserId);
        var model = new Web.Models.PostDetailViewModel
        {
            Post = post,
            Comments = comments,
            CommentThreads = BuildCommentThreads(comments),
            Categories = categories
                .Where(category => post.CategoryIds.Contains(category.Id))
                .ToList(),
            CurrentUserName = currentUser?.FullName ?? User.Identity?.Name ?? string.Empty,
            CurrentUserEmail = currentUser?.Email ?? User.FindFirstValue(ClaimTypes.Email) ?? string.Empty
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
    public async Task<IActionResult> AddComment(
        string slug,
        string content,
        int? parentCommentId = null)
    {
        if (string.IsNullOrWhiteSpace(content) || content.Length > 2000)
        {
            TempData["CommentError"] = "El comentario debe tener entre 1 y 2000 caracteres.";
            return RedirectToPostComments(slug);
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

        if (parentCommentId is int replyToId)
        {
            var parentComment = await _commentService.GetByIdAsync(replyToId);
            if (parentComment is null ||
                parentComment.PostId != post.Id ||
                (!parentComment.IsApproved && !User.IsInRole("Admin")))
            {
                TempData["CommentError"] = "No se puede responder a ese comentario.";
                return RedirectToPostComments(slug);
            }
        }

        await _commentService.CreateAsync(new CreateCommentDto(
            post.Id,
            authorId,
            content,
            false,
            parentCommentId));

        TempData["CommentSubmitted"] = "Tu comentario se envió y está pendiente de aprobación.";
        return RedirectToPostComments(slug);
    }

    private IActionResult RedirectToPostComments(string slug)
    {
        var postUrl = Url.RouteUrl("post-detail", new { slug })
            ?? throw new InvalidOperationException("The public post route could not be generated.");

        return Redirect($"{postUrl}#comments");
    }

    private static List<CommentThreadViewModel> BuildCommentThreads(
        IReadOnlyCollection<CommentDto> comments)
    {
        var threads = comments.ToDictionary(
            comment => comment.Id,
            comment => new CommentThreadViewModel { Comment = comment });
        var roots = new List<CommentThreadViewModel>();

        foreach (var comment in comments)
        {
            var thread = threads[comment.Id];
            if (comment.ParentCommentId is int parentId &&
                parentId != comment.Id &&
                threads.TryGetValue(parentId, out var parent))
            {
                parent.Replies.Add(thread);
            }
            else
            {
                roots.Add(thread);
            }
        }

        return roots;
    }

    [HttpGet("/privacidad", Name = "privacy-policy")]
    public IActionResult Privacy()
    {
        return View();
    }

    [HttpGet("/cookies", Name = "cookie-policy")]
    public IActionResult Cookies()
    {
        return View();
    }

    [HttpGet("/aviso-legal", Name = "legal-notice")]
    public IActionResult LegalNotice()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
