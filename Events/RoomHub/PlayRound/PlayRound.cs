using System.Text.Json;
using cinema.Models;
using cinema.Repositories.RoomRepository;
using FluentResults;

namespace cinema.Events.RoomHub.PlayRound;

public class PlayRound(IRoomRepository _roomRepository, IHttpClientFactory _httpClientFactory, IConfiguration _configuration) : IPlayRoundEvent
{
    public static readonly string JWT_ENCRYPTION_KEY_ID = "jwt_key";

    private void SetUpNextRound(MoviePickRoom room)
    {
        room.CurrentRound += 1;
        room.PlayersConfirmed = [];
    }

    public async Task<IResult<MoviePickRoom>> Exec(PlayRoundRequest request, Player caller)
    {
        // buscar room
        var roomFindOp = await _roomRepository.FindByIdentifier(request.RoomCode);
        var roomEntry = roomFindOp.Value;
        if (roomFindOp.IsFailed || roomEntry is null)
        {
            return await Task.Run(() => Result.Fail<MoviePickRoom>(roomFindOp.Errors));
        }

        if (caller.Identifier != roomEntry.OwnerId)
        {
            return await Task.Run(() => Result.Fail<MoviePickRoom>([RoomHubErrors.UNAUTHORIZED_ACTION]));
        }

        if (!roomEntry.CanBeginRound)
        {
            return await Task.Run(() => Result.Fail<MoviePickRoom>([RoomHubErrors.ROOM_CONFIRMATIONS_PENDING]));
        }

        List<string> languages = roomEntry.RoomPreferences.Languages;
        List<string> genres = roomEntry.RoomPreferences.Genres;
        List<string> streamings = roomEntry.RoomPreferences.Streamings;
        if (roomEntry.AllowUserPreferences)
        {
            roomEntry.PlayersConfirmed.ForEach(player =>
            {
                languages.AddRange(player.Preferences.Languages.Where(l => !languages.Contains(l)));
                genres.AddRange(player.Preferences.Genres.Where(g => !genres.Contains(g)));
                streamings.AddRange(player.Preferences.Streamings.Where(s => !streamings.Contains(s)));
            });
        }

        var client = _httpClientFactory.CreateClient();
        string? authKey = _configuration[cinema.RoomHub.API_AUTHKEY_ENV_KEY];
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {authKey}");

        string? baseUrl = _configuration[cinema.RoomHub.API_ENDPOINT_ENV_KEY];
        string url = $"{baseUrl}?language={string.Join(",", languages)}&with_genres={string.Join(",", genres)}"; // &with_watch_providers={string.Join(",", streamings)}
        HttpResponseMessage response = await client.GetAsync(url);
        if (!response.IsSuccessStatusCode)
        {
            string a = await response.Content.ReadAsStringAsync();
            return Result.Fail<MoviePickRoom>(RoomHubErrors.SUGGESTIONS_API_ERROR);
        };

        string responseContent = await response.Content.ReadAsStringAsync();
        var movieList = JsonSerializer.Deserialize<SuggestionsResult>(responseContent);
        roomEntry.Suggestions = movieList;
        this.SetUpNextRound(roomEntry);
        return Result.Ok<MoviePickRoom>(roomEntry);
    }
}