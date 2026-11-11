using Application.DTOs;
using Application.Interfaces;
using AutoMapper;
using Domain.Models;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

public class PostService : IPostService
{
    private readonly AppDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly IArticleContentSanitizer _articleContentSanitizer;

    public PostService(
        AppDbContext dbContext,
        IMapper mapper,
        IArticleContentSanitizer articleContentSanitizer)
    {
        _dbContext = dbContext;
        _mapper = mapper;
        _articleContentSanitizer = articleContentSanitizer;
    }

    public async Task<PostDto> CreateAsync(string authorId, CreatePostDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);
        ValidatePostFields(dto.Title, dto.Slug, dto.Content, authorId);
        var articles = PrepareArticles(dto.Articles);
        ArgumentNullException.ThrowIfNull(dto.CategoryIds);

        var authorExists = await _dbContext.Users
            .AnyAsync(user => user.Id == authorId);

        if (!authorExists)
        {
            throw new ArgumentException("Author not found.", nameof(authorId));
        }

        var categoryIds = dto.CategoryIds.Distinct().ToList();
        if (categoryIds.Count == 0)
        {
            throw new ArgumentException("At least one category is required.", nameof(dto));
        }

        await ValidateCategoryIdsAsync(categoryIds);

        if (await _dbContext.Posts.AnyAsync(post => post.Slug == dto.Slug))
        {
            throw new ArgumentException("A post with this slug already exists.", nameof(dto));
        }

        await using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            var post = _mapper.Map<Post>(dto);
            post.AuthorId = authorId;
            post.CreatedAt = DateTime.UtcNow;
            post.PublishedAt = post.IsPublished ? post.CreatedAt : null;

            _dbContext.Posts.Add(post);
            await _dbContext.SaveChangesAsync();

            foreach (var articleDto in articles)
            {
                var article = _mapper.Map<Article>(articleDto);
                article.PostId = post.Id;
                _dbContext.Articles.Add(article);
            }

            foreach (var categoryId in categoryIds)
            {
                _dbContext.PostCategories.Add(new PostCategory
                {
                    PostId = post.Id,
                    CategoryId = categoryId
                });
            }

            await _dbContext.SaveChangesAsync();

            var createdPost = await CreatePostQuery()
                .SingleAsync(existingPost => existingPost.Id == post.Id);
            var result = _mapper.Map<PostDto>(createdPost);

            await transaction.CommitAsync();
            return result;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task DeleteAsync(string authorId, int id)
    {
        var post = await _dbContext.Posts
            .SingleOrDefaultAsync(post => post.Id == id);

        if (post is null)
        {
            throw new KeyNotFoundException($"Post with ID {id} was not found.");
        }

        EnsureAuthor(post, authorId);

        _dbContext.Posts.Remove(post);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<IEnumerable<PostDto>> GetAllAsync()
    {
        var posts = await CreatePostQuery()
            .AsNoTracking()
            .OrderByDescending(post => post.CreatedAt)
            .ToListAsync();

        return _mapper.Map<IEnumerable<PostDto>>(posts);
    }

    public async Task<PostDto?> GetByIdAsync(int id)
    {
        var post = await CreatePostQuery()
            .AsNoTracking()
            .SingleOrDefaultAsync(post => post.Id == id);

        return post is null ? null : _mapper.Map<PostDto>(post);
    }

    public async Task<PostDto?> GetBySlugAsync(string slug)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(slug);

        var post = await CreatePostQuery()
            .AsNoTracking()
            .SingleOrDefaultAsync(post => post.Slug == slug);

        return post is null ? null : _mapper.Map<PostDto>(post);
    }

    public async Task UpdateAsync(string authorId, int id, UpdatePostDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var post = await _dbContext.Posts
            .Include(post => post.PostCategories)
            .Include(post => post.Articles)
            .SingleOrDefaultAsync(post => post.Id == id);

        if (post is null)
        {
            throw new KeyNotFoundException($"Post with ID {id} was not found.");
        }

        EnsureAuthor(post, authorId);
        ValidateOptionalPostFields(dto);
        List<ArticleDto>? articles = null;
        if (dto.Articles is not null)
        {
            articles = PrepareArticles(dto.Articles);
        }

        if (dto.Slug is not null &&
            await _dbContext.Posts.AnyAsync(otherPost =>
                otherPost.Id != id && otherPost.Slug == dto.Slug))
        {
            throw new ArgumentException("A post with this slug already exists.", nameof(dto));
        }

        if (dto.CategoryIds is not null)
        {
            var categoryIds = dto.CategoryIds.Distinct().ToList();
            await ValidateCategoryIdsAsync(categoryIds);

            post.PostCategories.Clear();
            foreach (var categoryId in categoryIds)
            {
                post.PostCategories.Add(new PostCategory
                {
                    PostId = post.Id,
                    CategoryId = categoryId
                });
            }
        }

        if (articles is not null)
        {
            post.Articles.Clear();
            foreach (var articleDto in articles)
            {
                var article = _mapper.Map<Article>(articleDto);
                article.PostId = post.Id;
                post.Articles.Add(article);
            }
        }

        var wasPublished = post.IsPublished;
        _mapper.Map(dto, post);
        if (dto.IsPublished.HasValue)
        {
            post.PublishedAt = dto.IsPublished.Value
                ? (wasPublished ? post.PublishedAt : DateTime.UtcNow)
                : null;
        }

        post.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();
    }

    private IQueryable<Post> CreatePostQuery()
    {
        return _dbContext.Posts
            .Include(post => post.Author)
            .Include(post => post.Articles)
            .Include(post => post.PostCategories)
            .Include(post => post.Comments);
    }

    private async Task ValidateCategoryIdsAsync(IReadOnlyCollection<int> categoryIds)
    {
        if (categoryIds.Any(categoryId => categoryId <= 0))
        {
            throw new ArgumentException("Category IDs must be greater than zero.", nameof(categoryIds));
        }

        if (categoryIds.Count == 0)
        {
            return;
        }

        var existingCategoryCount = await _dbContext.Categories
            .CountAsync(category => categoryIds.Contains(category.Id));

        if (existingCategoryCount != categoryIds.Count)
        {
            throw new ArgumentException("One or more categories were not found.", nameof(categoryIds));
        }
    }

    private List<ArticleDto> PrepareArticles(IReadOnlyCollection<ArticleDto>? articles)
    {
        ArgumentNullException.ThrowIfNull(articles);

        var populatedArticles = articles
            .Where(article => article is not null && !string.IsNullOrWhiteSpace(article.Content))
            .ToList();

        if (populatedArticles.Any(article => string.IsNullOrWhiteSpace(article.ContentType)))
        {
            throw new ArgumentException("Every article with content must have a content type.", nameof(articles));
        }

        var sanitizedArticles = populatedArticles
            .Select(article => article with
            {
                Content = string.Equals(article.ContentType, "Code", StringComparison.OrdinalIgnoreCase)
                    ? article.Content
                    : _articleContentSanitizer.Sanitize(article.Content)
            })
            .ToList();

        if (sanitizedArticles.Count == 0 ||
            sanitizedArticles.Any(article => string.IsNullOrWhiteSpace(article.Content)))
        {
            throw new ArgumentException("Enter content for at least one valid article.", nameof(articles));
        }

        return sanitizedArticles;
    }

    private static void ValidatePostFields(string title, string slug, string content, string authorId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(slug);
        ArgumentException.ThrowIfNullOrWhiteSpace(content);
        ArgumentException.ThrowIfNullOrWhiteSpace(authorId);
    }

    private static void ValidateOptionalPostFields(UpdatePostDto dto)
    {
        if (dto.Title is not null && string.IsNullOrWhiteSpace(dto.Title))
        {
            throw new ArgumentException("Post title cannot be empty.", nameof(dto));
        }

        if (dto.Slug is not null && string.IsNullOrWhiteSpace(dto.Slug))
        {
            throw new ArgumentException("Post slug cannot be empty.", nameof(dto));
        }

        if (dto.Content is not null && string.IsNullOrWhiteSpace(dto.Content))
        {
            throw new ArgumentException("Post content cannot be empty.", nameof(dto));
        }
    }

    private static void EnsureAuthor(Post post, string authorId)
    {
        if (!string.Equals(post.AuthorId, authorId, StringComparison.Ordinal))
        {
            throw new UnauthorizedAccessException("Only the post author can modify or delete this post.");
        }
    }
}
