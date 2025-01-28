using cinema.Models;
using cinema.Repositories.RoomRepository;
using FluentResults;

namespace cinema.Events.RoomHub.ChangeOwnership;

public class ChangeOwnership(IRoomRepository _roomRepository) : IChangeOwnershipEvent
{
    public async Task<IResult<MoviePickRoom>> Exec(ChangeOwnershipRequest request, Player? player)
    {
        var activeRoomOp = await _roomRepository.FindByIdentifier(request.RoomCode);
        if (activeRoomOp.IsFailed)
        {
            return await Task.Run(() => Result.Fail<MoviePickRoom>(RoomHubErrors.ACTIVE_ROOM_NOT_FOUND));
        }

        MoviePickRoom activeRoom = activeRoomOp.Value!;
        bool isRoomEmpty = activeRoom.PlayersConnectedTotal is 0;
        if (isRoomEmpty)
        {
            return await _roomRepository.Delete(activeRoom);
        }

        player ??= activeRoom.PlayersConnected.First();
        activeRoom.OwnerId = player.Identifier;
        var updateOp = await _roomRepository.Update(activeRoom);
        if (updateOp.IsFailed)
        {
            return await Task.Run(() => Result.Fail<MoviePickRoom>(updateOp.Errors));
        }
        return await Task.Run(() => Result.Ok(updateOp.Value));
    }
}