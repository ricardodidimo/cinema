using cinema.Models;

namespace cinema.Events.RoomHub.ConfirmRound;


public class ConfirmRoundResponse
{
    public required string ConfirmedPlayerId { get; set; }
    public required string ConfirmedPlayerName { get; set; }
    public bool CanBeginRound { get; set; }
    public int ConfirmationsTotal {get; set;}
    public int CurrentRound {get; set;}

    public static ConfirmRoundResponse ToResponse(MoviePickRoom moviePickRoom, Player playerConfirmed)
    {
        return new()
        {
            ConfirmedPlayerId = playerConfirmed.Identifier,
            ConfirmedPlayerName = playerConfirmed.Name,
            CanBeginRound = moviePickRoom.CanBeginRound,
            ConfirmationsTotal = moviePickRoom.PlayersConfirmedTotal,
            CurrentRound = moviePickRoom.CurrentRound
        };
    }
}