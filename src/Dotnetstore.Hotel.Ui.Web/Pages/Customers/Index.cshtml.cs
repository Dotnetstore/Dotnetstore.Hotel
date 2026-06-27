using Dotnetstore.Hotel.Shared.Sdk.Client;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Customer;
using Dotnetstore.Hotel.Ui.Web.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Dotnetstore.Hotel.Ui.Web.Pages.Customers;

[Authorize(Roles = "administrator,superuser,desk")]
public class IndexModel(IHotelClient hotelClient) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    public IReadOnlyList<CustomerDto> Customers { get; private set; } = [];

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        var accessToken = await HttpContext.GetTokenAsync(AuthTokenNames.AccessToken);
        if (string.IsNullOrEmpty(accessToken))
        {
            return Challenge();
        }

        Customers = await hotelClient.ListCustomersAsync(Search, accessToken, cancellationToken);
        return Page();
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var accessToken = await HttpContext.GetTokenAsync(AuthTokenNames.AccessToken);
        if (string.IsNullOrEmpty(accessToken))
        {
            return Challenge();
        }

        var result = await hotelClient.DeleteCustomerAsync(id, accessToken, cancellationToken);
        TempData[result.Succeeded ? "StatusMessage" : "ErrorMessage"] = result.Succeeded
            ? "Customer deleted."
            : string.Join(" ", result.Errors);

        return RedirectToPage(new { Search });
    }
}
