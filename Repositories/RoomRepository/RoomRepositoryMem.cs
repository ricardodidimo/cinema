using System.Collections.Concurrent;
using cinema.Events.RoomHub;
using cinema.Models;
using FluentResults;

namespace cinema.Repositories.RoomRepository;

public class RoomRepositoryMem : IRoomRepository
{
    static readonly ConcurrentDictionary<string, MoviePickRoom> Rooms = new();

    public async Task<IResult<MoviePickRoom?>> FindByIdentifier(string roomId)
    {
        var RoomEntry = Rooms.FirstOrDefault((groups) => groups.Key == roomId);
        if (RoomEntry.Value is null)
        {
            return await Task.Run(() => Result.Fail<MoviePickRoom>(RoomHubErrors.ACTIVE_ROOM_NOT_FOUND));
        }

        return await Task.Run(() => Result.Ok(RoomEntry.Value));
    }

    public async Task<IResult<MoviePickRoom>> Add(MoviePickRoom room)
    {
        if (!Rooms.TryAdd(room.Identifier, room))
        {
            return await Task.Run(() => Result.Fail<MoviePickRoom>(RoomHubErrors.UNABLE_CREATE_ROOM));
        }

        return await Task.Run(() => Result.Ok(room));
    }

    public async Task<IResult<MoviePickRoom>> AddPlayer(MoviePickRoom room, Player player)
    {
        room.PlayersConnected.Add(player);
        return await Task.Run(() => Result.Ok(room));
    }

    public async Task<IResult<MoviePickRoom>> RemovePlayer(MoviePickRoom room, Player player)
    {
        room.PlayersConnected.Remove(player);
        return await Task.Run(() => Result.Ok(room));
    }

    public async Task<IResult<MoviePickRoom>> ConfirmRound(MoviePickRoom room, Player player)
    {        
        room.PlayersConfirmed.Add(player);
        return await Task.Run(() => Result.Ok(room));
    }
}