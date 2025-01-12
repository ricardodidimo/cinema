using cinema.Models;
using FluentResults;

namespace cinema.Events.RoomHub.ConfirmRound;

public interface IConfirmRoundEvent
{
    public Task<IResult<MoviePickRoom>> Exec(ConfirmRoundRequest request, Player identity);
}