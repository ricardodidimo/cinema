namespace cinema.Models;

public record FilterPreferences
{
    public required List<string> Languages { get; set; }
    public required List<string> Genres { get; set; }
    public required List<string> Streamings { get; set; }
}