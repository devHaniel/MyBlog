namespace Application.DTOs;

public record UpdateCategoryDto(
    string? Name,
    string? Slug,
    string? Description);
