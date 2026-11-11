namespace Application.DTOs;

public record ArticleDto(
    int Id,
    string Content,
    string ContentType,
    int Order,
    int PostId);
