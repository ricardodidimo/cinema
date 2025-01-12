namespace cinema.Models;

public record SuggestionsResult
{
    public int page { get; set; }
    public required List<object> results { get; set; }
}