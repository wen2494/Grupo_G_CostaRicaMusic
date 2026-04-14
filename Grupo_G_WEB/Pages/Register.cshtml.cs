using Grupo_G_WEB.Models;
using Grupo_G_WEB.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Grupo_G_WEB.Pages;

public class RegisterModel(IMusicCatalogService catalogService) : PageModel
{
    [BindProperty]
    public string NombreUsuario { get; set; } = string.Empty;

    [BindProperty]
    public string Contrasena { get; set; } = string.Empty;

    [BindProperty]
    public string ConfirmarContrasena { get; set; } = string.Empty;

    [BindProperty]
    public string? Email { get; set; }

    [BindProperty]
    public string? NombreCompleto { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    public string? MensajeError { get; private set; }

    public IActionResult OnGet()
    {
        return HttpContext.Session.GetInt32("UserId") is not null
            ? RedirectToPage("/Index")
            : Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrWhiteSpace(NombreUsuario) || string.IsNullOrWhiteSpace(Contrasena))
        {
            MensajeError = "Escribe usuario y contrasena.";
            return Page();
        }

        if (Contrasena != ConfirmarContrasena)
        {
            MensajeError = "Las contrasenas no coinciden.";
            return Page();
        }

        var result = await catalogService.RegisterAsync(new RegisterRequest
        {
            NombreUsuario = NombreUsuario.Trim(),
            Contrasena = Contrasena,
            Email = Email?.Trim(),
            NombreCompleto = NombreCompleto?.Trim()
        });

        if (result.User is null)
        {
            MensajeError = result.Error ?? "No se pudo crear la cuenta.";
            return Page();
        }

        HttpContext.Session.SetInt32("UserId", result.User.Id);
        HttpContext.Session.SetString("UserName", result.User.NombreUsuario);
        HttpContext.Session.SetString("DisplayName", result.User.NombreCompleto ?? result.User.NombreUsuario);
        HttpContext.Session.SetString("Token", result.User.Token ?? string.Empty);

        return !string.IsNullOrWhiteSpace(ReturnUrl) && Url.IsLocalUrl(ReturnUrl)
            ? LocalRedirect(ReturnUrl)
            : RedirectToPage("/Index");
    }
}
