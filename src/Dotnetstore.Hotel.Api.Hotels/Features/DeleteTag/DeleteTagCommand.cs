using Dotnetstore.Hotel.Shared.Cqrs;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Tag;

namespace Dotnetstore.Hotel.Api.Hotels.Features.DeleteTag;

public record DeleteTagCommand(Guid Id) : ICommand<DeleteTagResponse>;
