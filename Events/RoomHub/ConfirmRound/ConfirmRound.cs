using cinema.Models;
using cinema.Repositories.RoomRepository;
using FluentResults;

namespace cinema.Events.RoomHub.ConfirmRound;


public class ConfirmRound(IRoomRepository _roomRepository) : IConfirmRoundEvent
{
    public async Task<IResult<ConfirmRoundResponse>> Exec(ConfirmRoundRequest request, Player player)
    {
        var activeRoomOp = await _roomRepository.FindByIdentifier(request.RoomCode);
        var activeRoom = activeRoomOp.Value;
        if (activeRoomOp.IsFailed || activeRoom is null)
        {
            return await Task.Run(() => Result.Fail<ConfirmRoundResponse>(RoomHubErrors.ACTIVE_ROOM_NOT_FOUND));
        }

        var connectedPlayer = activeRoom.PlayersConnected.Find(p => p.Identifier == player.Identifier);
        if (connectedPlayer is null)
        {
            return await Task.Run(() => Result.Fail<ConfirmRoundResponse>(RoomHubErrors.PLAYER_NOT_FOUND));
        }

        if (connectedPlayer.IsReady)
        {
            return await Task.Run(() => Result.Fail<ConfirmRoundResponse>(RoomHubErrors.PLAYER_ALREADY_CONFIRMED));
        }

        connectedPlayer.IsReady = true;
        var updateOperation = await _roomRepository.Update(activeRoom);
        if (updateOperation.IsFailed)
        {
            return await Task.Run(() => Result.Fail<ConfirmRoundResponse>(RoomHubErrors.UNABLE_UPDATE_ROOM));
        }

        return await Task.Run(() => Result.Ok(ConfirmRoundResponse.ToResponse(activeRoom, connectedPlayer)));
    }
}