namespace Application.DTOs;

public record UserDto(
    string Id,
    string FullName,
    string UserName,
    string? Email,
    DateTime CreatedAt,
    int PostCount,
    int CommentCount);
