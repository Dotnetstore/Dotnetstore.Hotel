using Dotnetstore.Hotel.Shared.Sdk.Client;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Tag;
using Dotnetstore.Hotel.Ui.Web.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Dotnetstore.Hotel.Ui.Web.Pages.Tags;

[Authorize(Roles = "administrator,superuser")]
public class IndexModel(IHotelClient hotelClient) : PageModel
{
    public IReadOnlyList<TagDto> Tags { get; private set; } = [];

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        var accessToken = await HttpContext.GetTokenAsync(AuthTokenNames.AccessToken);
        if (string.IsNullOrEmpty(accessToken))
        {
            return Challenge();
        }

        Tags = await hotelClient.ListTagsAsync(accessToken, cancellationToken);
        return Page();
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var accessToken = await HttpContext.GetTokenAsync(AuthTokenNames.AccessToken);
        if (string.IsNullOrEmpty(accessToken))
        {
            return Challenge();
        }

        var result = await hotelClient.DeleteTagAsync(id, accessToken, cancellationToken);
        TempData[result.Succeeded ? "StatusMessage" : "ErrorMessage"] = result.Succeeded
            ? "Tag deleted."
            : string.Join(" ", result.Errors);

        return RedirectToPage();
    }
}
