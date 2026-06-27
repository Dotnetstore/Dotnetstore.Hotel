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
public class EditModel(IHotelClient hotelClient) : PageModel
{
    public IReadOnlyList<string> IdentificationTypes { get; } = ["Passport", "NationalId"];

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public int CustomerNumber { get; private set; }

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
    public DateOnly DateOfBirth { get; set; }

    [BindProperty, Required]
    public string Nationality { get; set; } = string.Empty;

    [BindProperty]
    public string? Notes { get; set; }

    public IReadOnlyList<string> Errors { get; private set; } = [];

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        var accessToken = await HttpContext.GetTokenAsync(AuthTokenNames.AccessToken);
        if (string.IsNullOrEmpty(accessToken))
        {
            return Challenge();
        }

        var customer = await hotelClient.GetCustomerAsync(Id, accessToken, cancellationToken);
        if (customer is null)
        {
            return NotFound();
        }

        CustomerNumber = customer.CustomerNumber;
        FullName = customer.FullName;
        IdentificationType = customer.IdentificationType;
        IdentificationNumber = customer.IdentificationNumber;
        Street = customer.Address.Street;
        City = customer.Address.City;
        PostalCode = customer.Address.PostalCode;
        Country = customer.Address.Country;
        PhoneNumber = customer.PhoneNumber;
        Email = customer.Email;
        DateOfBirth = customer.DateOfBirth;
        Nationality = customer.Nationality;
        Notes = customer.Notes;

        return Page();
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
        var request = new UpdateCustomerRequest(FullName, IdentificationType, IdentificationNumber, address, PhoneNumber, Email, DateOfBirth, Nationality, Notes);
        var result = await hotelClient.UpdateCustomerAsync(Id, request, accessToken, cancellationToken);
        if (result.Customer is null)
        {
            Errors = result.Errors;
            return Page();
        }

        TempData["StatusMessage"] = $"Customer '{result.Customer.FullName}' updated.";
        return RedirectToPage("/Customers/Index");
    }
}
