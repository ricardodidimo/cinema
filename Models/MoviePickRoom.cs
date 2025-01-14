namespace cinema.Models;

public class MoviePickRoom
{
    public required string Identifier { get; set; }
    public required string OwnerId { get; set; }
    public required List<Player> PlayersConnected { get; set; }
    public int PlayersConnectedTotal => this.PlayersConnected.Count;
    public required List<Player> PlayersConfirmed { get; set; }
    public int PlayersConfirmedTotal => this.PlayersConnected.Count;
    public bool CanBeginRound => this.PlayersConnected.Count == PlayersConfirmed.Count;
    public int CurrentRound { get; set; }
    public required bool AllowUserPreferences { get; set; }
    public required FilterPreferences RoomPreferences { get; set; }
    public int MaxPlayers { get; set; }
    public int MaxSuggestions { get; set; }
    public SuggestionsResult? Suggestions { get; set; }
}