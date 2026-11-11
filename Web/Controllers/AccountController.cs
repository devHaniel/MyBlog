using Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using System.Net.Mail;
using System.Text;
using Web.ViewModels;
using Web.Services;

namespace Web.Controllers;

public class AccountController : Controller
{
    private readonly SignInManager<User> _signInManager;
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IEmailSender _emailSender;
    private readonly EmailOptions _emailOptions;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        SignInManager<User> signInManager,
        UserManager<User> userManager,
        RoleManager<IdentityRole> roleManager,
        IEmailSender emailSender,
        IOptions<EmailOptions> emailOptions,
        ILogger<AccountController> logger)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _roleManager = roleManager;
        _emailSender = emailSender;
        _emailOptions = emailOptions.Value;
        _logger = logger;
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Register(string? email = null)
    {
        return View(new RegisterViewModel { Email = email ?? string.Empty });
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var existingUser = await _userManager.FindByEmailAsync(model.Email);
        if (existingUser is not null)
        {
            ModelState.AddModelError(nameof(model.Email), "No se pudo crear la cuenta con esos datos.");
            return View(model);
        }

        const string visitorRole = "Visitor";
        if (!await _roleManager.RoleExistsAsync(visitorRole))
        {
            var roleResult = await _roleManager.CreateAsync(new IdentityRole(visitorRole));
            if (!roleResult.Succeeded)
            {
                AddIdentityErrors(roleResult);
                return View(model);
            }
        }

        var user = new User
        {
            FullName = model.FullName.Trim(),
            UserName = model.Email.Trim(),
            Email = model.Email.Trim(),
            EmailConfirmed = false,
            CreatedAt = DateTime.UtcNow
        };

        var createResult = await _userManager.CreateAsync(user, model.Password);
        if (!createResult.Succeeded)
        {
            AddIdentityErrors(createResult);
            return View(model);
        }

        var roleAssignment = await _userManager.AddToRoleAsync(user, visitorRole);
        if (!roleAssignment.Succeeded)
        {
            var deleteResult = await _userManager.DeleteAsync(user);
            if (!deleteResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Could not remove a partially registered user: {string.Join("; ", deleteResult.Errors.Select(error => error.Description))}");
            }

            AddIdentityErrors(roleAssignment);
            return View(model);
        }

        try
        {
            var publicBaseUrl = new Uri(_emailOptions.PublicBaseUrl, UriKind.Absolute);
            if (publicBaseUrl.Scheme is not ("http" or "https"))
            {
                throw new InvalidOperationException("Email:PublicBaseUrl must use HTTP or HTTPS.");
            }

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var confirmationUrl = QueryHelpers.AddQueryString(
                new Uri(publicBaseUrl, "/Account/ConfirmEmail").ToString(),
                new Dictionary<string, string?>
                {
                    ["userId"] = user.Id,
                    ["token"] = encodedToken
                });

            await _emailSender.SendConfirmationEmailAsync(user.Email!, confirmationUrl);
            return View("ConfirmationPending");
        }
        catch (Exception exception) when (
            exception is InvalidOperationException or SmtpException or UriFormatException or FormatException)
        {
            _logger.LogError(exception, "Could not send an email confirmation to {Email}.", user.Email);
            var deleteResult = await _userManager.DeleteAsync(user);
            if (!deleteResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Could not roll back a registration after email delivery failed: {string.Join("; ", deleteResult.Errors.Select(error => error.Description))}",
                    exception);
            }

            ModelState.AddModelError(
                string.Empty,
                "No pudimos enviar el correo de confirmación. Inténtalo más tarde.");
            return View(model);
        }
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> ConfirmEmail(string? userId, string? token)
    {
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(token))
        {
            return View("EmailConfirmation", false);
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return View("EmailConfirmation", false);
        }

        try
        {
            var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
            var result = await _userManager.ConfirmEmailAsync(user, decodedToken);
            return View("EmailConfirmation", result.Succeeded);
        }
        catch (FormatException)
        {
            return View("EmailConfirmation", false);
        }
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.FindByEmailAsync(model.Email);
        var result = user is null
            ? Microsoft.AspNetCore.Identity.SignInResult.Failed
            : await _signInManager.PasswordSignInAsync(
                user,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: true);

        if (result.Succeeded)
        {
            if (!string.IsNullOrWhiteSpace(model.ReturnUrl) &&
                Url.IsLocalUrl(model.ReturnUrl))
            {
                return LocalRedirect(model.ReturnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        if (result.IsNotAllowed)
        {
            ModelState.AddModelError(string.Empty, "Confirma tu correo electrónico antes de iniciar sesión.");
            return View(model);
        }

        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
        return View(model);
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    private void AddIdentityErrors(IdentityResult result)
    {
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }
    }
}
