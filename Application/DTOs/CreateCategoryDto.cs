namespace Application.DTOs;

public record CreateCategoryDto(
    string Name,
    string Slug,
    string? Description);
