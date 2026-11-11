namespace Application.DTOs;

public record UpdatePostDto(
    string? Title,
    string? Slug,
    string? Content,
    string? Excerpt,
    string? CoverImage,
    bool? IsPublished,
    List<ArticleDto>? Articles,
    List<int>? CategoryIds);
