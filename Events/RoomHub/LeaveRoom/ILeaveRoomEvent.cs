using cinema.Models;
using FluentResults;

namespace cinema.Events.RoomHub.LeaveRoom;

public interface ILeaveRoomEvent
{
    public Task<IResult<MoviePickRoom>> Exec(LeaveRoomRequest request, Player player);
}