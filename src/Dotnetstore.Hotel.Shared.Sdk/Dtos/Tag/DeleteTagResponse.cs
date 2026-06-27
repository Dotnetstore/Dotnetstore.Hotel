namespace Dotnetstore.Hotel.Shared.Sdk.Dtos.Tag;

public record DeleteTagResponse(bool Succeeded, IReadOnlyList<string> Errors);
