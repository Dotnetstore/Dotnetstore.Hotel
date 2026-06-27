using System.ComponentModel.DataAnnotations;
using Dotnetstore.Hotel.Shared.Sdk.Client;
using Dotnetstore.Hotel.Shared.Sdk.Dtos;
using Dotnetstore.Hotel.Ui.Web.Options;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

namespace Dotnetstore.Hotel.Ui.Web.Pages;

[Authorize]
public class HotelModel(IHotelClient hotelClient, IOptions<HotelUiOptions> hotelOptions) : PageModel
{
    public HotelDto? Hotel { get; private set; }

    [BindProperty, Required]
    public string Name { get; set; } = string.Empty;

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

    [BindProperty, Required, EmailAddress]
    public string ContactEmail { get; set; } = string.Empty;

    [BindProperty]
    public string? Website { get; set; }

    [BindProperty]
    public string AmenitiesCsv { get; set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        Hotel = await hotelClient.GetHotelAsync(hotelOptions.Value.Id, cancellationToken);
        if (Hotel is null)
        {
            return NotFound();
        }

        PopulateFormFrom(Hotel);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            Hotel = await hotelClient.GetHotelAsync(hotelOptions.Value.Id, cancellationToken);
            return Page();
        }

        var amenities = AmenitiesCsv.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        var request = new UpdateHotelRequest(
            Name,
            new AddressDto(Street, City, PostalCode, Country),
            new ContactInfoDto(PhoneNumber, ContactEmail, string.IsNullOrWhiteSpace(Website) ? null : Website),
            amenities);

        await hotelClient.UpdateHotelAsync(hotelOptions.Value.Id, request, cancellationToken);

        TempData["StatusMessage"] = "Hotel details updated.";
        return RedirectToPage();
    }

    private void PopulateFormFrom(HotelDto hotel)
    {
        Name = hotel.Name;
        Street = hotel.Address.Street;
        City = hotel.Address.City;
        PostalCode = hotel.Address.PostalCode;
        Country = hotel.Address.Country;
        PhoneNumber = hotel.ContactInfo.PhoneNumber;
        ContactEmail = hotel.ContactInfo.Email;
        Website = hotel.ContactInfo.Website;
        AmenitiesCsv = string.Join(", ", hotel.Amenities);
    }
}
