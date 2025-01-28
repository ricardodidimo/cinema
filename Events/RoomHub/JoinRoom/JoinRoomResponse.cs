using cinema.Models;

namespace cinema.Events.RoomHub.CreateRoom;


public class JoinRoomResponse
{
    public required string Identifier { get; set; }
    public required List<Player> PlayersConnected { get; set; }
    public int PlayersConnectedTotal { get; set; }
    public List<Player>? PlayersConfirmed { get; set; }
    public int PlayersConfirmedTotal { get; set; }
    public int MaxPlayers { get; set; }
    public int MaxSuggestions { get; set; }
    public required FilterPreferences Preferences {get; set;}

    public static JoinRoomResponse ToResponse(MoviePickRoom moviePickRoom)
    {
        return new()
        {
            Identifier = moviePickRoom.Identifier,
            Preferences = moviePickRoom.RoomPreferences,
            MaxPlayers = moviePickRoom.MaxPlayers,
            MaxSuggestions = moviePickRoom.MaxSuggestions,
            PlayersConnected = moviePickRoom.PlayersConnected,
            PlayersConnectedTotal = moviePickRoom.PlayersConnectedTotal,
            PlayersConfirmed = moviePickRoom.PlayersConfirmed,
            PlayersConfirmedTotal = moviePickRoom.PlayersConfirmedTotal,
        };
    }
}