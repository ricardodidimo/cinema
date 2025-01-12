using cinema.Models;

namespace cinema.Events.RoomHub.Subscribe;

public record SubscribeRequest
{
    public required string Name { get; set; }
    public required string Avatar { get; set; }
    public required FilterPreferences Preferences { get; set; }
}