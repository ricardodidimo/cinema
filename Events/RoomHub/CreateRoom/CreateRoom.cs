using System.Text.Json.Serialization;
using cinema.Models;
using cinema.Repositories.RoomRepository;
using FluentResults;
using Microsoft.AspNetCore.SignalR;

namespace cinema.Events.RoomHub.CreateRoom;


public class CreateRoom(IRoomRepository _RoomRepository) : ICreateRoomEvent
{
    private readonly string NanoIdAlphabet = "_0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private async Task<string> GenerateRandomRoomCode()
    {
        return await NanoidDotNet.Nanoid.GenerateAsync(NanoIdAlphabet, 6);
    }

    public async Task<IResult<MoviePickRoom>> Exec(CreateRoomRequest createReq, Player identity)
    {
        var roomId = await GenerateRandomRoomCode();
        MoviePickRoom roomConfiguration = new()
        {
            Identifier = roomId,
            MaxPlayers = createReq.MaxPlayers,
            MaxSuggestions = createReq.MaxSuggestions,
            RoomPreferences = createReq.Preferences,
            AllowUserPreferences = createReq.AllowUserPreferences,
            OwnerId = identity.Identifier,
            PlayersConnected = [identity],
            PlayersConfirmed = []
        };

        await _RoomRepository.Add(roomConfiguration);
        return await Task.Run(() => Result.Ok(roomConfiguration));
    }
}