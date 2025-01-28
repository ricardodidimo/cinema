using Newtonsoft.Json;
using cinema.Models;
using cinema.Repositories.RoomRepository;
using FluentResults;

namespace cinema.Events.RoomHub.PlayRound;

public class PlayRound(IRoomRepository _roomRepository, IHttpClientFactory _httpClientFactory, IConfiguration _configuration) : IPlayRoundEvent
{
    private void SetUpNextRound(MoviePickRoom room)
    {
        room.CurrentRound += 1;
        room.PlayersConnected.ForEach(p => p.IsReady = false);

        _roomRepository.Update(room);
    }

    public async Task<IResult<MoviePickRoom>> Exec(PlayRoundRequest request, Player caller)
    {
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
            roomEntry.PlayersConnected.ForEach(player =>
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
        if (baseUrl is null)
        {
            return await Task.Run(() => Result.Fail<MoviePickRoom>([RoomHubErrors.SUGGESTIONS_API_ERROR]));
        }

        string urlTemplate = "{0}?language={1}&with_genres={2}&with_watch_providers={3}&page={4}";
        string url = string.Format(urlTemplate, baseUrl,
            string.Join(",", languages),
            string.Join(",", genres),
            string.Join(",", streamings),
            roomEntry.CurrentRound);

        HttpResponseMessage response = await client.GetAsync(url);
        string responseContent = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            return Result.Fail<MoviePickRoom>(RoomHubErrors.SUGGESTIONS_API_ERROR);
        }

        SuggestionsResult? movieList = JsonConvert.DeserializeObject<SuggestionsResult>(responseContent);
        if (movieList is null)
        {
            return Result.Fail<MoviePickRoom>(RoomHubErrors.SUGGESTIONS_API_ERROR);
        }

        roomEntry.Suggestions = movieList;
        this.SetUpNextRound(roomEntry);
        return Result.Ok<MoviePickRoom>(roomEntry);
    }
}