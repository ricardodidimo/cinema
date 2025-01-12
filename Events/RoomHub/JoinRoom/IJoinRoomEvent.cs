using cinema.Models;
using FluentResults;

namespace cinema.Events.RoomHub.JoinRoom;

public interface IJoinRoomEvent
{
    public Task<IResult<MoviePickRoom>> Exec(JoinRoomRequest request, Player player);
}