using Dotnetstore.Hotel.Shared.Cqrs;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Tag;

namespace Dotnetstore.Hotel.Api.Hotels.Features.UpdateTag;

public record UpdateTagCommand(Guid Id, string Name) : ICommand<UpdateTagResponse>;
