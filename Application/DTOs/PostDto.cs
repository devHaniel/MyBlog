namespace Application.DTOs;

public record PostDto(
    int Id,
    string Title,
    string Slug,
    string Content,
    List<ArticleDto> Articles,
    string? Excerpt,
    string? CoverImage,
    bool IsPublished,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    DateTime? PublishedAt,
    string AuthorId,
    string AuthorName,
    List<int> CategoryIds,
    int CommentCount);
