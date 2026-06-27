using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Dotnetstore.Hotel.Shared.Sdk.Dtos;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Booking;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Customer;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Equipment;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Room;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Tag;

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

    public async Task<DeleteEquipmentResponse> DeleteEquipmentAsync(Guid id, string bearerToken, CancellationToken cancellationToken = default)
    {
        using var requestMessage = new HttpRequestMessage(HttpMethod.Delete, $"/api/equipment/{id}");
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

        using var response = await httpClient.SendAsync(requestMessage, cancellationToken);
        if (response.StatusCode == HttpStatusCode.NoContent)
        {
            return new DeleteEquipmentResponse(true, []);
        }

        var result = await response.Content.ReadFromJsonAsync<DeleteEquipmentResponse>(cancellationToken);
        return result ?? new DeleteEquipmentResponse(false, ["Unexpected empty response"]);
    }

    public async Task<IReadOnlyList<RoomDto>> ListRoomsAsync(string bearerToken, CancellationToken cancellationToken = default)
    {
        using var requestMessage = new HttpRequestMessage(HttpMethod.Get, "/api/rooms");
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

        using var response = await httpClient.SendAsync(requestMessage, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<IReadOnlyList<RoomDto>>(cancellationToken) ?? [];
    }

    public async Task<RoomDto?> GetRoomAsync(Guid id, string bearerToken, CancellationToken cancellationToken = default)
    {
        using var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"/api/rooms/{id}");
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

        using var response = await httpClient.SendAsync(requestMessage, cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<RoomDto>(cancellationToken);
    }

    public async Task<CreateRoomResponse> CreateRoomAsync(CreateRoomRequest request, string bearerToken, CancellationToken cancellationToken = default)
    {
        using var requestMessage = new HttpRequestMessage(HttpMethod.Post, "/api/rooms")
        {
            Content = JsonContent.Create(request),
        };
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

        using var response = await httpClient.SendAsync(requestMessage, cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<CreateRoomResponse>(cancellationToken);
        return result ?? new CreateRoomResponse(null, ["Unexpected empty response"]);
    }

    public async Task<UpdateRoomResponse> UpdateRoomAsync(Guid id, UpdateRoomRequest request, string bearerToken, CancellationToken cancellationToken = default)
    {
        using var requestMessage = new HttpRequestMessage(HttpMethod.Put, $"/api/rooms/{id}")
        {
            Content = JsonContent.Create(request),
        };
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

        using var response = await httpClient.SendAsync(requestMessage, cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<UpdateRoomResponse>(cancellationToken);
        return result ?? new UpdateRoomResponse(null, ["Unexpected empty response"]);
    }

    public async Task<DeleteRoomResponse> DeleteRoomAsync(Guid id, string bearerToken, CancellationToken cancellationToken = default)
    {
        using var requestMessage = new HttpRequestMessage(HttpMethod.Delete, $"/api/rooms/{id}");
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

        using var response = await httpClient.SendAsync(requestMessage, cancellationToken);
        if (response.StatusCode == HttpStatusCode.NoContent)
        {
            return new DeleteRoomResponse(true, []);
        }

        var result = await response.Content.ReadFromJsonAsync<DeleteRoomResponse>(cancellationToken);
        return result ?? new DeleteRoomResponse(false, ["Unexpected empty response"]);
    }

    public async Task<IReadOnlyList<RoomDto>> SearchRoomsAsync(
        DateOnly? checkInDate,
        DateOnly? checkOutDate,
        IReadOnlyCollection<Guid> equipmentIds,
        IReadOnlyCollection<Guid> tagIds,
        string bearerToken,
        CancellationToken cancellationToken = default)
    {
        var queryParameters = new List<string>();
        if (checkInDate is { } checkIn)
        {
            queryParameters.Add($"checkInDate={checkIn:yyyy-MM-dd}");
        }

        if (checkOutDate is { } checkOut)
        {
            queryParameters.Add($"checkOutDate={checkOut:yyyy-MM-dd}");
        }

        queryParameters.AddRange(equipmentIds.Select(id => $"equipmentIds={id}"));
        queryParameters.AddRange(tagIds.Select(id => $"tagIds={id}"));

        var queryString = queryParameters.Count > 0 ? $"?{string.Join("&", queryParameters)}" : string.Empty;
        using var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"/api/rooms/search{queryString}");
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

        using var response = await httpClient.SendAsync(requestMessage, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<IReadOnlyList<RoomDto>>(cancellationToken) ?? [];
    }

    public async Task<IReadOnlyList<TagDto>> ListTagsAsync(string bearerToken, CancellationToken cancellationToken = default)
    {
        using var requestMessage = new HttpRequestMessage(HttpMethod.Get, "/api/tags");
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

        using var response = await httpClient.SendAsync(requestMessage, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<IReadOnlyList<TagDto>>(cancellationToken) ?? [];
    }

    public async Task<TagDto?> GetTagAsync(Guid id, string bearerToken, CancellationToken cancellationToken = default)
    {
        using var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"/api/tags/{id}");
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

        using var response = await httpClient.SendAsync(requestMessage, cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TagDto>(cancellationToken);
    }

    public async Task<CreateTagResponse> CreateTagAsync(CreateTagRequest request, string bearerToken, CancellationToken cancellationToken = default)
    {
        using var requestMessage = new HttpRequestMessage(HttpMethod.Post, "/api/tags")
        {
            Content = JsonContent.Create(request),
        };
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

        using var response = await httpClient.SendAsync(requestMessage, cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<CreateTagResponse>(cancellationToken);
        return result ?? new CreateTagResponse(null, ["Unexpected empty response"]);
    }

    public async Task<UpdateTagResponse> UpdateTagAsync(Guid id, UpdateTagRequest request, string bearerToken, CancellationToken cancellationToken = default)
    {
        using var requestMessage = new HttpRequestMessage(HttpMethod.Put, $"/api/tags/{id}")
        {
            Content = JsonContent.Create(request),
        };
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

        using var response = await httpClient.SendAsync(requestMessage, cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<UpdateTagResponse>(cancellationToken);
        return result ?? new UpdateTagResponse(null, ["Unexpected empty response"]);
    }

    public async Task<DeleteTagResponse> DeleteTagAsync(Guid id, string bearerToken, CancellationToken cancellationToken = default)
    {
        using var requestMessage = new HttpRequestMessage(HttpMethod.Delete, $"/api/tags/{id}");
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

        using var response = await httpClient.SendAsync(requestMessage, cancellationToken);
        if (response.StatusCode == HttpStatusCode.NoContent)
        {
            return new DeleteTagResponse(true, []);
        }

        var result = await response.Content.ReadFromJsonAsync<DeleteTagResponse>(cancellationToken);
        return result ?? new DeleteTagResponse(false, ["Unexpected empty response"]);
    }

    public async Task<IReadOnlyList<CustomerDto>> ListCustomersAsync(string? searchTerm, string bearerToken, CancellationToken cancellationToken = default)
    {
        var queryString = string.IsNullOrWhiteSpace(searchTerm) ? string.Empty : $"?search={Uri.EscapeDataString(searchTerm)}";
        using var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"/api/customers{queryString}");
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

        using var response = await httpClient.SendAsync(requestMessage, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<IReadOnlyList<CustomerDto>>(cancellationToken) ?? [];
    }

    public async Task<CustomerDto?> GetCustomerAsync(Guid id, string bearerToken, CancellationToken cancellationToken = default)
    {
        using var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"/api/customers/{id}");
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

        using var response = await httpClient.SendAsync(requestMessage, cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<CustomerDto>(cancellationToken);
    }

    public async Task<CreateCustomerResponse> CreateCustomerAsync(CreateCustomerRequest request, string bearerToken, CancellationToken cancellationToken = default)
    {
        using var requestMessage = new HttpRequestMessage(HttpMethod.Post, "/api/customers")
        {
            Content = JsonContent.Create(request),
        };
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

        using var response = await httpClient.SendAsync(requestMessage, cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<CreateCustomerResponse>(cancellationToken);
        return result ?? new CreateCustomerResponse(null, ["Unexpected empty response"]);
    }

    public async Task<UpdateCustomerResponse> UpdateCustomerAsync(Guid id, UpdateCustomerRequest request, string bearerToken, CancellationToken cancellationToken = default)
    {
        using var requestMessage = new HttpRequestMessage(HttpMethod.Put, $"/api/customers/{id}")
        {
            Content = JsonContent.Create(request),
        };
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

        using var response = await httpClient.SendAsync(requestMessage, cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<UpdateCustomerResponse>(cancellationToken);
        return result ?? new UpdateCustomerResponse(null, ["Unexpected empty response"]);
    }

    public async Task<DeleteCustomerResponse> DeleteCustomerAsync(Guid id, string bearerToken, CancellationToken cancellationToken = default)
    {
        using var requestMessage = new HttpRequestMessage(HttpMethod.Delete, $"/api/customers/{id}");
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

        using var response = await httpClient.SendAsync(requestMessage, cancellationToken);
        if (response.StatusCode == HttpStatusCode.NoContent)
        {
            return new DeleteCustomerResponse(true, []);
        }

        var result = await response.Content.ReadFromJsonAsync<DeleteCustomerResponse>(cancellationToken);
        return result ?? new DeleteCustomerResponse(false, ["Unexpected empty response"]);
    }

    public async Task<IReadOnlyList<BookingDto>> ListBookingsAsync(Guid? customerId, string bearerToken, CancellationToken cancellationToken = default)
    {
        var queryString = customerId is { } id ? $"?customerId={id}" : string.Empty;
        using var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"/api/bookings{queryString}");
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

        using var response = await httpClient.SendAsync(requestMessage, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<IReadOnlyList<BookingDto>>(cancellationToken) ?? [];
    }

    public async Task<BookingDto?> GetBookingAsync(Guid id, string bearerToken, CancellationToken cancellationToken = default)
    {
        using var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"/api/bookings/{id}");
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

        using var response = await httpClient.SendAsync(requestMessage, cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<BookingDto>(cancellationToken);
    }

    public async Task<CreateBookingResponse> CreateBookingAsync(CreateBookingRequest request, string bearerToken, CancellationToken cancellationToken = default)
    {
        using var requestMessage = new HttpRequestMessage(HttpMethod.Post, "/api/bookings")
        {
            Content = JsonContent.Create(request),
        };
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

        using var response = await httpClient.SendAsync(requestMessage, cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<CreateBookingResponse>(cancellationToken);
        return result ?? new CreateBookingResponse(null, ["Unexpected empty response"]);
    }

    public Task<BookingActionResponse> CancelBookingAsync(Guid id, string bearerToken, CancellationToken cancellationToken = default)
        => PostBookingActionAsync($"/api/bookings/{id}/cancel", bearerToken, cancellationToken);

    public Task<BookingActionResponse> CheckInBookingAsync(Guid id, string bearerToken, CancellationToken cancellationToken = default)
        => PostBookingActionAsync($"/api/bookings/{id}/check-in", bearerToken, cancellationToken);

    public Task<BookingActionResponse> CheckOutBookingAsync(Guid id, string bearerToken, CancellationToken cancellationToken = default)
        => PostBookingActionAsync($"/api/bookings/{id}/check-out", bearerToken, cancellationToken);

    private async Task<BookingActionResponse> PostBookingActionAsync(string path, string bearerToken, CancellationToken cancellationToken)
    {
        using var requestMessage = new HttpRequestMessage(HttpMethod.Post, path);
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

        using var response = await httpClient.SendAsync(requestMessage, cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<BookingActionResponse>(cancellationToken);
        return result ?? new BookingActionResponse(false, ["Unexpected empty response"]);
    }
}
