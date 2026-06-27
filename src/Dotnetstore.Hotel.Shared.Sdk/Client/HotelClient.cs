using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Dotnetstore.Hotel.Shared.Sdk.Dtos;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Equipment;

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

    public async Task<IReadOnlyList<EquipmentDto>> ListEquipmentAsync(string bearerToken, CancellationToken cancellationToken = default)
    {
        using var requestMessage = new HttpRequestMessage(HttpMethod.Get, "/api/equipment");
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

        using var response = await httpClient.SendAsync(requestMessage, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<IReadOnlyList<EquipmentDto>>(cancellationToken) ?? [];
    }

    public async Task<EquipmentDto?> GetEquipmentAsync(Guid id, string bearerToken, CancellationToken cancellationToken = default)
    {
        using var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"/api/equipment/{id}");
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

        using var response = await httpClient.SendAsync(requestMessage, cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<EquipmentDto>(cancellationToken);
    }

    public async Task<CreateEquipmentResponse> CreateEquipmentAsync(CreateEquipmentRequest request, string bearerToken, CancellationToken cancellationToken = default)
    {
        using var requestMessage = new HttpRequestMessage(HttpMethod.Post, "/api/equipment")
        {
            Content = JsonContent.Create(request),
        };
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

        using var response = await httpClient.SendAsync(requestMessage, cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<CreateEquipmentResponse>(cancellationToken);
        return result ?? new CreateEquipmentResponse(null, ["Unexpected empty response"]);
    }

    public async Task<UpdateEquipmentResponse> UpdateEquipmentAsync(Guid id, UpdateEquipmentRequest request, string bearerToken, CancellationToken cancellationToken = default)
    {
        using var requestMessage = new HttpRequestMessage(HttpMethod.Put, $"/api/equipment/{id}")
        {
            Content = JsonContent.Create(request),
        };
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

        using var response = await httpClient.SendAsync(requestMessage, cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<UpdateEquipmentResponse>(cancellationToken);
        return result ?? new UpdateEquipmentResponse(null, ["Unexpected empty response"]);
    }

    public async Task<bool> DeleteEquipmentAsync(Guid id, string bearerToken, CancellationToken cancellationToken = default)
    {
        using var requestMessage = new HttpRequestMessage(HttpMethod.Delete, $"/api/equipment/{id}");
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

        using var response = await httpClient.SendAsync(requestMessage, cancellationToken);
        return response.StatusCode == HttpStatusCode.NoContent;
    }
}
