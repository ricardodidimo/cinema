namespace cinema.Events.RoomHub.CastVote;

public class CastVoteRequest
{
    public required string RoomCode { get; set; }
    public required string MovieCode { get; set; }
}