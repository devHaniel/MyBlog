namespace Application.DTOs;

public record CreateUserDto(
    string FullName,
    string UserName,
    string? Email,
    string? Password);
