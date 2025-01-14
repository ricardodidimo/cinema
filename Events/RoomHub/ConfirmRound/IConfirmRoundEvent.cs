using cinema.Models;
using FluentResults;

namespace cinema.Events.RoomHub.ConfirmRound;

public interface IConfirmRoundEvent
{
    public Task<IResult<ConfirmRoundResponse>> Exec(ConfirmRoundRequest request, Player identity);
}