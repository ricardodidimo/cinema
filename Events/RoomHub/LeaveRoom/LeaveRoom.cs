using cinema.Models;
using cinema.Repositories.RoomRepository;
using FluentResults;

namespace cinema.Events.RoomHub.LeaveRoom;


public class LeaveRoom(IRoomRepository _roomRepository) : ILeaveRoomEvent
{
    public async Task<IResult<MoviePickRoom>> Exec(LeaveRoomRequest request, Player player)
    {
        var roomFindOp = await _roomRepository.FindByIdentifier(request.RoomCode);
        var roomEntry = roomFindOp.Value;
        if (roomFindOp.IsFailed || roomEntry is null)
        {
            return await Task.Run(() => Result.Fail<MoviePickRoom>(roomFindOp.Errors));
        }

        var leaveRoom = await _roomRepository.RemovePlayer(roomEntry, player);
        MoviePickRoom room = leaveRoom.Value;
        if (leaveRoom.IsFailed)
        {
            return Result.Fail<MoviePickRoom>(leaveRoom.Errors);
        }

        bool isRoomEmpty = room.PlayersConnectedTotal is 0;
        if (isRoomEmpty)
        {
            return await _roomRepository.Delete(room);
        }

        return Result.Ok(room);
    }
}