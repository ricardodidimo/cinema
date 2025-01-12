using cinema.Models;
using FluentResults;

namespace cinema.Events.RoomHub.CreateRoom;

public interface ICreateRoomEvent
{
    public Task<IResult<MoviePickRoom>> Exec(CreateRoomRequest request, Player identity);
}