namespace Dotnetstore.Hotel.Shared.Sdk.Dtos.Tag;

public record CreateTagResponse(TagDto? Tag, IReadOnlyList<string> Errors);
