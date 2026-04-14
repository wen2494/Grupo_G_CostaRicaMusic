using Grupo_G_WEB.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Grupo_G_WEB.Pages;

public class LoginModel(IMusicCatalogService catalogService) : PageModel
{
    [BindProperty]
    public string NombreUsuario { get; set; } = string.Empty;

    [BindProperty]
    public string Contrasena { get; set; } = string.Empty;

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

        var user = await catalogService.LoginAsync(NombreUsuario.Trim(), Contrasena);
        if (user is null)
        {
            MensajeError = "Usuario o contrasena incorrectos.";
            return Page();
        }

        HttpContext.Session.SetInt32("UserId", user.Id);
        HttpContext.Session.SetString("UserName", user.NombreUsuario);
        HttpContext.Session.SetString("DisplayName", user.NombreCompleto ?? user.NombreUsuario);
        HttpContext.Session.SetString("Token", user.Token ?? string.Empty);

        return !string.IsNullOrWhiteSpace(ReturnUrl) && Url.IsLocalUrl(ReturnUrl)
            ? LocalRedirect(ReturnUrl)
            : RedirectToPage("/Index");
    }
}
