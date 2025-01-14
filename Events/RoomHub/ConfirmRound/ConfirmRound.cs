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

        var confirmation = activeRoom.PlayersConfirmed.FirstOrDefault(p => p.Identifier == player.Identifier);
        if (confirmation is not null)
        {
            return await Task.Run(() => Result.Fail<ConfirmRoundResponse>(RoomHubErrors.PLAYER_ALREADY_CONFIRMED));
        }

        var confirmOperation = await _roomRepository.ConfirmRound(activeRoom, player);
        if (confirmOperation.IsFailed)
        {
            return await Task.Run(() => Result.Fail<ConfirmRoundResponse>(confirmOperation.Errors));
        }

        return await Task.Run(() => Result.Ok(ConfirmRoundResponse.ToResponse(activeRoom, player)));
    }
}