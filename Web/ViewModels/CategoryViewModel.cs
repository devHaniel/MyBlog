using Application.DTOs;

namespace Web.ViewModels;

public class CategoryIndexViewModel
{
    public IEnumerable<CategoryDto> Categories { get; set; } = new List<CategoryDto>();
}

public class CategoryDetailsViewModel
{
    public CategoryDto Category { get; set; } = new(
        0,
        string.Empty,
        string.Empty,
        null,
        0);
}

public class CreateCategoryViewModel
{
    public CreateCategoryDto Dto { get; set; } = new(
        string.Empty,
        string.Empty,
        null);
}

public class EditCategoryViewModel
{
    public int Id { get; set; }
    public UpdateCategoryDto Dto { get; set; } = new(null, null, null);
}