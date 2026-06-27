using Dotnetstore.Hotel.Api.Hotels.Persistence;
using Dotnetstore.Hotel.Shared.Cqrs;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Tag;

namespace Dotnetstore.Hotel.Api.Hotels.Features.DeleteTag;

public class DeleteTagCommandHandler(ITagRepository tagRepository, IRoomRepository roomRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteTagCommand, DeleteTagResponse>
{
    public async Task<DeleteTagResponse> HandleAsync(DeleteTagCommand command, CancellationToken cancellationToken)
    {
        var tag = await tagRepository.GetByIdAsync(command.Id, cancellationToken);
        if (tag is null)
        {
            return new DeleteTagResponse(false, ["Tag not found."]);
        }

        var roomsUsingTag = await roomRepository.CountUsingTagAsync(command.Id, cancellationToken);
        if (roomsUsingTag > 0)
        {
            return new DeleteTagResponse(false, [$"{roomsUsingTag} room(s) still use this tag. Remove it from those rooms first."]);
        }

        tagRepository.Remove(tag);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return new DeleteTagResponse(true, []);
    }
}
