namespace Application.DTOs;

public record UpdateArticleDto(
    string? Content,
    string? ContentType,
    int? Order);
