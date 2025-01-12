using cinema.Models;

namespace cinema.Events.RoomHub.CreateRoom;


public class LeaveRoomResponse
{
    public required string RoomIdentifier { get; set; }

    public static LeaveRoomResponse ToResponse(MoviePickRoom moviePickRoom)
    {
        return new()
        {
            RoomIdentifier = moviePickRoom.Identifier,
        };
    }
}