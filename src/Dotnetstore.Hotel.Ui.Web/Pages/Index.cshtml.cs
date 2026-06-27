using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Dotnetstore.Hotel.Ui.Web.Pages;

public class IndexModel : PageModel
{
    public IActionResult OnGet() => RedirectToPage("/Hotel");
}
