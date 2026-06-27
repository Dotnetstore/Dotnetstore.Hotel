using Dotnetstore.Hotel.Api.Hotels.Persistence;
using Dotnetstore.Hotel.Shared.Cqrs;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Tag;

namespace Dotnetstore.Hotel.Api.Hotels.Features.UpdateTag;

public class UpdateTagCommandHandler(ITagRepository tagRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateTagCommand, UpdateTagResponse>
{
    public async Task<UpdateTagResponse> HandleAsync(UpdateTagCommand command, CancellationToken cancellationToken)
    {
        var tag = await tagRepository.GetByIdAsync(command.Id, cancellationToken);
        if (tag is null)
        {
            return new UpdateTagResponse(null, ["Tag not found."]);
        }

        if (string.IsNullOrWhiteSpace(command.Name))
        {
            return new UpdateTagResponse(null, ["Tag name is required."]);
        }

        if (await tagRepository.ExistsByNameAsync(command.Name, command.Id, cancellationToken))
        {
            return new UpdateTagResponse(null, [$"Tag '{command.Name}' already exists."]);
        }

        tag.UpdateDetails(command.Name);
        tagRepository.Update(tag);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new UpdateTagResponse(TagDtoMapper.ToDto(tag), []);
    }
}
