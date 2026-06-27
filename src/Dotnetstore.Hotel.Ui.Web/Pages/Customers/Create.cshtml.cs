using System.ComponentModel.DataAnnotations;
using Dotnetstore.Hotel.Shared.Sdk.Client;
using Dotnetstore.Hotel.Shared.Sdk.Dtos;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Customer;
using Dotnetstore.Hotel.Ui.Web.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Dotnetstore.Hotel.Ui.Web.Pages.Customers;

[Authorize(Roles = "administrator,superuser,desk")]
public class CreateModel(IHotelClient hotelClient) : PageModel
{
    // Mirrors Api.Hotels' IdentificationTypes constants - duplicated rather than shared, same trade-off
    // already accepted for RoomStatuses/role literals duplicated between the UI and Api.Hotels.
    public IReadOnlyList<string> IdentificationTypes { get; } = ["Passport", "NationalId"];

    [BindProperty, Required]
    public string FullName { get; set; } = string.Empty;

    [BindProperty, Required]
    public string IdentificationType { get; set; } = "Passport";

    [BindProperty, Required]
    public string IdentificationNumber { get; set; } = string.Empty;

    [BindProperty, Required]
    public string Street { get; set; } = string.Empty;

    [BindProperty, Required]
    public string City { get; set; } = string.Empty;

    [BindProperty, Required]
    public string PostalCode { get; set; } = string.Empty;

    [BindProperty, Required]
    public string Country { get; set; } = string.Empty;

    [BindProperty, Required]
    public string PhoneNumber { get; set; } = string.Empty;

    [BindProperty, Required]
    public string Email { get; set; } = string.Empty;

    [BindProperty, Required]
    public DateOnly DateOfBirth { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-30));

    [BindProperty, Required]
    public string Nationality { get; set; } = string.Empty;

    [BindProperty]
    public string? Notes { get; set; }

    public IReadOnlyList<string> Errors { get; private set; } = [];

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var accessToken = await HttpContext.GetTokenAsync(AuthTokenNames.AccessToken);
        if (string.IsNullOrEmpty(accessToken))
        {
            return Challenge();
        }

        var address = new AddressDto(Street, City, PostalCode, Country);
        var request = new CreateCustomerRequest(FullName, IdentificationType, IdentificationNumber, address, PhoneNumber, Email, DateOfBirth, Nationality, Notes);
        var result = await hotelClient.CreateCustomerAsync(request, accessToken, cancellationToken);
        if (result.Customer is null)
        {
            Errors = result.Errors;
            return Page();
        }

        TempData["StatusMessage"] = $"Customer '{result.Customer.FullName}' created.";
        return RedirectToPage("/Customers/Index");
    }
}
