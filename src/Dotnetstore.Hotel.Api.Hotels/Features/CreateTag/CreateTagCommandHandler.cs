using Dotnetstore.Hotel.Api.Hotels.Persistence;
using Dotnetstore.Hotel.Shared.Cqrs;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Tag;
using TagEntity = Dotnetstore.Hotel.Api.Hotels.Domain.Tag;

namespace Dotnetstore.Hotel.Api.Hotels.Features.CreateTag;

public class CreateTagCommandHandler(ITagRepository tagRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<CreateTagCommand, CreateTagResponse>
{
    public async Task<CreateTagResponse> HandleAsync(CreateTagCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Name))
        {
            return new CreateTagResponse(null, ["Tag name is required."]);
        }

        if (await tagRepository.ExistsByNameAsync(command.Name, excludingId: null, cancellationToken))
        {
            return new CreateTagResponse(null, [$"Tag '{command.Name}' already exists."]);
        }

        var tag = TagEntity.Create(Guid.NewGuid(), command.Name);
        await tagRepository.AddAsync(tag, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateTagResponse(TagDtoMapper.ToDto(tag), []);
    }
}
