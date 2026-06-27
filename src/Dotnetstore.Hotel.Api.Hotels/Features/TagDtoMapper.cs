using Dotnetstore.Hotel.Shared.Sdk.Dtos.Tag;
using TagEntity = Dotnetstore.Hotel.Api.Hotels.Domain.Tag;

namespace Dotnetstore.Hotel.Api.Hotels.Features;

internal static class TagDtoMapper
{
    public static TagDto ToDto(TagEntity tag) => new(tag.Id, tag.Name);
}
