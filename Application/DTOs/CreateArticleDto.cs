namespace Application.DTOs;

public record CreateArticleDto(
    string Content,
    string ContentType,
    int Order,
    int PostId);
