using Dotnetstore.Hotel.Shared.Cqrs;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Tag;

namespace Dotnetstore.Hotel.Api.Hotels.Features.CreateTag;

public record CreateTagCommand(string Name) : ICommand<CreateTagResponse>;
