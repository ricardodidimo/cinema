using cinema.Models;

namespace cinema.Events.RoomHub.CreateRoom;


public class CreateRoomResponse
{
    public required string Identifier { get; set; }
    public required int MaxPlayers { get; set; }
    public required int MaxSuggestions { get; set; }
    public required FilterPreferences Preferences {get; set;}

    public static CreateRoomResponse ToResponse(MoviePickRoom moviePickRoom)
    {
        return new()
        {
            Identifier = moviePickRoom.Identifier,
            Preferences = moviePickRoom.RoomPreferences,
            MaxPlayers = moviePickRoom.MaxPlayers,
            MaxSuggestions = moviePickRoom.MaxSuggestions
        };
    }
}