using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DedsiNative.AuthServer.Pages;

public class IndexModel : PageModel
{
    public bool IsAuthenticated => User.Identity?.IsAuthenticated == true;
    public string? UserName => User.Identity?.Name;

    public void OnGet()
    {
    }
}
