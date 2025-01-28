using cinema.Models;
using cinema.Repositories.RoomRepository;
using FluentResults;

namespace cinema.Events.RoomHub.CastVote;


public class CastVote(IRoomRepository _roomRepository) : ICastVoteEvent
{
    public async Task<IResult<CastVoteResponse>> Exec(CastVoteRequest request, Player player)
    {
        var activeRoomOp = await _roomRepository.FindByIdentifier(request.RoomCode);
        var activeRoom = activeRoomOp.Value;
        if (activeRoomOp.IsFailed || activeRoom is null)
        {
            return await Task.Run(() => Result.Fail<CastVoteResponse>(RoomHubErrors.ACTIVE_ROOM_NOT_FOUND));
        }

        var movieSuggestion = activeRoom.Suggestions?.Results.FirstOrDefault(s => s.Id.ToString() == request.MovieCode);
        if (movieSuggestion is null)
        { 
            return await Task.Run(() => Result.Fail<CastVoteResponse>(RoomHubErrors.ACTIVE_SUGGESTION_NOT_FOUND));
        }

        var confirmation = movieSuggestion.RoundVotes.FirstOrDefault(p => p.Identifier == player.Identifier);
        if (confirmation is not null)
        {
            return await Task.Run(() => Result.Fail<CastVoteResponse>(RoomHubErrors.PLAYER_ALREADY_VOTED));
        }

        movieSuggestion.RoundVotes.Add(player);
        var updateOperation = await _roomRepository.Update(activeRoom);
        if (updateOperation.IsFailed)
        {
            return await Task.Run(() => Result.Fail<CastVoteResponse>(updateOperation.Errors));
        }

        return await Task.Run(() => Result.Ok(CastVoteResponse.ToResponse(activeRoom, movieSuggestion, player)));
    }
}