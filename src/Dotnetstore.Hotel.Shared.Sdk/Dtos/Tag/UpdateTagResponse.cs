namespace Dotnetstore.Hotel.Shared.Sdk.Dtos.Tag;

public record UpdateTagResponse(TagDto? Tag, IReadOnlyList<string> Errors);
