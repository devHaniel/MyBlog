namespace Application.DTOs;

public record UpdateUserDto(
    string? FullName,
    string? UserName,
    string? Email,
    string? PhoneNumber);
