namespace Application.DTOs;

public record UpdateCommentDto(
    string? Content,
    bool? IsApproved);
