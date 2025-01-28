using cinema.Models;
using FluentResults;

namespace cinema.Events.RoomHub.ChangeOwnership;

public interface IChangeOwnershipEvent
{
    public Task<IResult<MoviePickRoom>> Exec(ChangeOwnershipRequest request, Player? identity);
}