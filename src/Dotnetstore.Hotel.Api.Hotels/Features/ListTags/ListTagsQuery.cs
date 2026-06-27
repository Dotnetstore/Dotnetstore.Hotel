using Dotnetstore.Hotel.Shared.Cqrs;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Tag;

namespace Dotnetstore.Hotel.Api.Hotels.Features.ListTags;

public record ListTagsQuery : IQuery<IReadOnlyList<TagDto>>;
