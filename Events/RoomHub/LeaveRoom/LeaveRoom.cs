using cinema.Models;
using cinema.Repositories.RoomRepository;
using FluentResults;

namespace cinema.Events.RoomHub.LeaveRoom;


public class LeaveRoom(IRoomRepository _RoomRepository) : ILeaveRoomEvent
{
    public async Task<IResult<MoviePickRoom>> Exec(LeaveRoomRequest request, Player player)
    {
        var roomFindOp = await _RoomRepository.FindByIdentifier(request.RoomCode);
        var roomEntry = roomFindOp.Value;
        if (roomFindOp.IsFailed || roomEntry is null)
        {
            return await Task.Run(() => Result.Fail<MoviePickRoom>(roomFindOp.Errors));
        }

        var leaveRoom = await _RoomRepository.RemovePlayer(roomEntry, player);
        if (leaveRoom.IsFailed)
        {
            return Result.Fail<MoviePickRoom>(leaveRoom.Errors);
        }

        return Result.Ok(leaveRoom.Value);
    }
}