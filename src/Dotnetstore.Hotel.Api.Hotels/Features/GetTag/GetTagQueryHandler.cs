using Dotnetstore.Hotel.Api.Hotels.Persistence;
using Dotnetstore.Hotel.Shared.Cqrs;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Tag;

namespace Dotnetstore.Hotel.Api.Hotels.Features.GetTag;

public class GetTagQueryHandler(ITagRepository tagRepository) : IQueryHandler<GetTagQuery, TagDto?>
{
    public async Task<TagDto?> HandleAsync(GetTagQuery query, CancellationToken cancellationToken)
    {
        var tag = await tagRepository.GetByIdAsync(query.Id, cancellationToken);
        return tag is null ? null : TagDtoMapper.ToDto(tag);
    }
}
