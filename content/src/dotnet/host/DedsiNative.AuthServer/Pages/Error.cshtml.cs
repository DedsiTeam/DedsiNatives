using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DedsiNative.AuthServer.Pages;

[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
[IgnoreAntiforgeryToken]
public class ErrorModel : PageModel
{
    public string? RequestId { get; set; }

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

    public string? Error { get; set; }
    public string? ErrorDescription { get; set; }

    public void OnGet(string? error = null, string? error_description = null)
    {
        RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        Error = error;
        ErrorDescription = error_description;
    }
}
