namespace Application.DTOs;

public record CreateCommentDto(
    int PostId,
    string AuthorId,
    string Content,
    bool IsApproved = true);
