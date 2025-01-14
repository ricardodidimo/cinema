using cinema.Models;
using FluentResults;

namespace cinema.Events.RoomHub.CastVote;

public interface ICastVoteEvent
{
    public Task<IResult<CastVoteResponse>> Exec(CastVoteRequest request, Player identity);
}