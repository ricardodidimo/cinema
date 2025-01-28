namespace cinema.Models;

public class MoviePickRoom
{
    public required string Identifier { get; set; }
    public required string OwnerId { get; set; }
    public required List<Player> PlayersConnected { get; set; }
    public int PlayersConnectedTotal => this.PlayersConnected.Count;
    public List<Player>? PlayersConfirmed => this.PlayersConnected.FindAll(p => p.IsReady);
    public int PlayersConfirmedTotal => this.PlayersConnected.Count(P => P.IsReady);
    public bool CanBeginRound => this.PlayersConnected.All(P => P.IsReady);
    public int CurrentRound { get; set; }
    public required bool AllowUserPreferences { get; set; }
    public required FilterPreferences RoomPreferences { get; set; }
    public int MaxPlayers { get; set; }
    public int MaxSuggestions { get; set; }
    public SuggestionsResult? Suggestions { get; set; }
}