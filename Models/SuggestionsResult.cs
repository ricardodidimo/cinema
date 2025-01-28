namespace cinema.Models;

public record SuggestionsResult
{
    public int Page { get; set; }
    public required List<Movie> Results { get; set; }
}