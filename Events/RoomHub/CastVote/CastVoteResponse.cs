using cinema.Models;

namespace cinema.Events.RoomHub.CastVote;

public class CastVoteResponse
{
    public required MoviePickRoom Room { get; set; }
    public required Movie Movie { get; set; }
    public required Player Player { get; set; }
    public bool RoomReachedConsensus => Movie.RoundVotes.Count == Room.PlayersConfirmedTotal;

    public static CastVoteResponse ToResponse(MoviePickRoom moviePickRoom, Movie movieVotedFor, Player voter)
    {
        return new()
        {
            Room = moviePickRoom,
            Movie = movieVotedFor,
            Player = voter
        };
    }
}