using System.Net;
using System.Net.Http.Json;
using Dotnetstore.Hotel.Shared.Sdk.Dtos;

namespace Dotnetstore.Hotel.Shared.Sdk.Client;

internal sealed class HotelClient(HttpClient httpClient) : IHotelClient
{
    public async Task<HotelDto?> GetHotelAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.GetAsync($"/api/hotels/{id}", cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<HotelDto>(cancellationToken);
    }

    public async Task<HotelDto> UpdateHotelAsync(Guid id, UpdateHotelRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PutAsJsonAsync($"/api/hotels/{id}", request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<HotelDto>(cancellationToken))!;
    }
}
