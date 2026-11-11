namespace Application.DTOs;

public record CommentDto(
    int Id,
    string Content,
    DateTime CreatedAt,
    bool IsApproved,
    int PostId,
    string AuthorId,
    string AuthorName);
