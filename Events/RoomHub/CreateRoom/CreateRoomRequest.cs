using cinema.Models;

namespace cinema.Events.RoomHub.CreateRoom;


public class CreateRoomRequest
{
    public required int MaxPlayers { get; set; }
    public required int MaxSuggestions { get; set; }
    public required bool AllowUserPreferences { get; set; }
    public required FilterPreferences Preferences { get; set; }
}