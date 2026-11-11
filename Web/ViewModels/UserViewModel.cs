using Application.DTOs;

namespace Web.ViewModels;

public class UserIndexViewModel
{
    public UserDto User { get; set; } = new(
        string.Empty,
        string.Empty,
        string.Empty,
        null,
        DateTime.UtcNow,
        0,
        0);
}

public class UserCreateViewModel
{
    public CreateUserDto Dto { get; set; } = new(
        string.Empty,
        string.Empty,
        null,
        null);
}

public class UserEditViewModel
{
    public string Id { get; set; } = string.Empty;
    public UpdateUserDto Dto { get; set; } = new(null, null, null, null);
}