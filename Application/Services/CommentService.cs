using Application.DTOs;
using Application.Interfaces;
using AutoMapper;
using Domain.Models;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

public class CommentService : ICommentService
{
    private readonly AppDbContext _dbContext;
    private readonly IMapper _mapper;

    public CommentService(AppDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }

    public async Task<CommentDto> CreateAsync(CreateCommentDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        if (dto.PostId <= 0)
        {
            throw new ArgumentException("Post ID must be greater than zero.", nameof(dto));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(dto.AuthorId);
        ArgumentException.ThrowIfNullOrWhiteSpace(dto.Content);

        var authorExists = await _dbContext.Users
            .AnyAsync(user => user.Id == dto.AuthorId);
        if (!authorExists)
        {
            throw new ArgumentException("Author not found.", nameof(dto));
        }

        var postExists = await _dbContext.Posts
            .AnyAsync(post => post.Id == dto.PostId);
        if (!postExists)
        {
            throw new ArgumentException("Post not found.", nameof(dto));
        }

        var comment = _mapper.Map<Comment>(dto);
        comment.CreatedAt = DateTime.UtcNow;

        _dbContext.Comments.Add(comment);
        await _dbContext.SaveChangesAsync();

        var createdComment = await CreateCommentQuery()
            .SingleAsync(existingComment => existingComment.Id == comment.Id);

        return _mapper.Map<CommentDto>(createdComment);
    }

    public async Task DeleteAsync(int id)
    {
        var comment = await _dbContext.Comments
            .SingleOrDefaultAsync(comment => comment.Id == id);

        if (comment is null)
        {
            throw new KeyNotFoundException($"Comment with ID {id} was not found.");
        }

        _dbContext.Comments.Remove(comment);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<CommentDto?> GetByIdAsync(int id)
    {
        var comment = await CreateCommentQuery()
            .AsNoTracking()
            .SingleOrDefaultAsync(comment => comment.Id == id);

        return comment is null ? null : _mapper.Map<CommentDto>(comment);
    }

    public async Task<IEnumerable<CommentDto>> GetByPostAsync(int postId)
    {
        var comments = await CreateCommentQuery()
            .AsNoTracking()
            .Where(comment => comment.PostId == postId)
            .OrderBy(comment => comment.CreatedAt)
            .ThenBy(comment => comment.Id)
            .ToListAsync();

        return _mapper.Map<IEnumerable<CommentDto>>(comments);
    }

    public async Task UpdateAsync(int id, UpdateCommentDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var comment = await _dbContext.Comments
            .SingleOrDefaultAsync(comment => comment.Id == id);

        if (comment is null)
        {
            throw new KeyNotFoundException($"Comment with ID {id} was not found.");
        }

        if (dto.Content is not null && string.IsNullOrWhiteSpace(dto.Content))
        {
            throw new ArgumentException("Comment content cannot be empty.", nameof(dto));
        }

        _mapper.Map(dto, comment);
        await _dbContext.SaveChangesAsync();
    }

    private IQueryable<Comment> CreateCommentQuery()
    {
        return _dbContext.Comments
            .Include(comment => comment.Author);
    }
}
