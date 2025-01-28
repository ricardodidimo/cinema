namespace cinema.Models;

public record Player
{
    public required string Identifier { get; set; }
    public required string Name { get; set; }
    public required string Avatar { get; set; }
    public required FilterPreferences Preferences { get; set; }
    public bool IsReady {get; set;}
}