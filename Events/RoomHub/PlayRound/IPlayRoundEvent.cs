using cinema.Models;
using FluentResults;

namespace cinema.Events.RoomHub.PlayRound;

public interface IPlayRoundEvent
{
    public Task<IResult<MoviePickRoom>> Exec(PlayRoundRequest request, Player caller);
}