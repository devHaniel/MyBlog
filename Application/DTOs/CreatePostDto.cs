namespace Application.DTOs;

public record CreatePostDto(
    string Title,
    string Slug,
    string Content,
    string? Excerpt,
    string? CoverImage,
    bool IsPublished,
    string AuthorId,
    List<ArticleDto> Articles,
    List<int> CategoryIds);
