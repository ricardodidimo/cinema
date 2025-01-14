namespace cinema.Models;

public record SuggestionsResult
{
    public int page { get; set; }
    public required List<Movie> results { get; set; }
}