using cinema.Models;
using cinema.Repositories.RoomRepository;
using FluentResults;

namespace cinema.Events.RoomHub.JoinRoom;

public class JoinRoom(IRoomRepository _RoomRepository) : IJoinRoomEvent
{
    public async Task<IResult<MoviePickRoom>> Exec(JoinRoomRequest request, Player player)
    {
        var roomFindOp = await _RoomRepository.FindByIdentifier(request.RoomCode);
        var roomEntry = roomFindOp.Value;
        if (roomFindOp.IsFailed || roomEntry is null)
        {
            return await Task.Run(() => Result.Fail<MoviePickRoom>(roomFindOp.Errors));
        }

        if (roomEntry.PlayersConnectedTotal == roomEntry.MaxPlayers)
        {
            return await Task.Run(() => Result.Fail<MoviePickRoom>(RoomHubErrors.ROOM_IS_FULL));
        }

        var joinRoom = await _RoomRepository.AddPlayer(roomEntry, player);
        if (joinRoom.IsFailed)
        {
            return Result.Fail<MoviePickRoom>(joinRoom.Errors);
        }

        return Result.Ok(joinRoom.Value);
    }
}