using Dotnetstore.Hotel.Api.Hotels.Persistence;
using Dotnetstore.Hotel.Shared.Cqrs;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Tag;

namespace Dotnetstore.Hotel.Api.Hotels.Features.ListTags;

public class ListTagsQueryHandler(ITagRepository tagRepository) : IQueryHandler<ListTagsQuery, IReadOnlyList<TagDto>>
{
    public async Task<IReadOnlyList<TagDto>> HandleAsync(ListTagsQuery query, CancellationToken cancellationToken)
    {
        var tags = await tagRepository.GetAllAsync(cancellationToken);
        return tags.Select(TagDtoMapper.ToDto).ToList();
    }
}
